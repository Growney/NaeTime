import network
import time
import asyncio
import commands as commands
from devices.rssi import ADCReader
from devices.rssi import PeakDetector
from devices.rx5808 import Rx5808RegisterCommunication
from node import LaneConfiguration
import machine
import comms

def frequency_to_delay_ms(frequency_hz):
    if frequency_hz <= 0:
        raise ValueError("Frequency must be greater than 0")
    return 1000 / frequency_hz

def calculate_delay_ms(last_trigger, delay_ms, current_time):
    if last_trigger == 0:
        return 0
    
    if(current_time - last_trigger >= delay_ms):
        return 0

    todelay = delay_ms - (current_time - last_trigger)
    return todelay

def calculate_minimum_delay(last_times, delay_ms):
    time_pointer = 0
    min_delay = delay_ms
    while(time_pointer < len(lane_timings)):
        current_time = time.ticks_ms()
        delay = calculate_delay_ms(last_times[time_pointer], delay_ms, current_time)
        if delay < min_delay:
            min_delay = delay
        time_pointer += 1
    return min_delay

async def command_loop():
    print("starting command loop")
    global running
    global node_id
    global transmit_delay_ms
    global polling_delay_ms
    global node_comms
    global rx_modules
    global lane_configurations

    while running:
        command = await node_comms.wait_for_command()
        try:
            if command is None:
                continue
            elif isinstance(command, commands.TuneLane):
                print("Tune command received Lane: "+str(command.lane)+" Frequency: "+str(command.frequency_in_mhz))
                if(command.lane < len(rx_modules) and command.lane >= 0 and rx_modules[command.lane].tune(command.frequency_in_mhz)):
                    lane_configurations[command.lane].frequency_in_mhz = command.frequency_in_mhz
                    print("Tune Successful")
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
                    print("Tune Failed")
            elif isinstance(command, commands.ConfigureLaneEntryThreshold):
                print("Configure Entry Threshold Lane: "+str(command.lane)+" Threshold: "+str(command.entry_threshold))
                if(command.lane < len(peak_detectors) and command.lane >= 0):
                    peak_detectors[command.lane].entry_threshold = command.entry_threshold
                    lane_configurations[command.lane].entry_threshold = command.entry_threshold
                    print("Entry Threshold Set")
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
            elif isinstance(command, commands.ConfigureLaneExitThreshold):
                print("Configure Exit Threshold Lane: "+str(command.lane)+" Threshold: "+str(command.exit_threshold))
                if(command.lane < len(peak_detectors) and command.lane >= 0):
                    peak_detectors[command.lane].exit_threshold = command.exit_threshold
                    lane_configurations[command.lane].exit_threshold = command.exit_threshold
                    print("Exit Threshold Set")
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
            elif isinstance(command, commands.ConfigureLaneEnabled):
                print("Configure Lane Enabled Lane: "+str(command.lane)+" Enabled: "+str(command.enabled))
                if(command.lane < len(lane_configurations) and command.lane >= 0):
                    lane_configurations[command.lane].is_enabled = command.enabled == 1
                    print("Lane Enabled Set")
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
            elif isinstance(command, commands.RequestLaneConfigurations):
                print("Lane Configuration Request Received for lanes", str(command.lanes))
                response_configs = []
                for lane_index in range(len(lane_configurations)):
                    response_configs.append(commands.LaneConfiguration(lane_configurations[lane_index].bandId,
                                                                      lane_configurations[lane_index].frequency_in_mhz, 
                                                                      lane_configurations[lane_index].entry_threshold, 
                                                                      lane_configurations[lane_index].exit_threshold))
                enabled_lanes = 0
                for lane_index in range(len(lane_configurations)):
                    if(lane_configurations[lane_index].is_enabled):
                        enabled_lanes |= 1 << lane_index

                node_comms.send_response_for_command(command)

            elif isinstance(command, commands.ConfigureNode):
                print("Configure command received")
                node_id = command.node_id
                transmit_delay_ms = frequency_to_delay_ms(command.transmit_frequency_hz)
                polling_delay_ms = frequency_to_delay_ms(command.polling_frequency_hz)
            else:
                print("Unknown command received")
        except Exception as e:
            print("command error: ",str(e))
    
async def transmission_loop():
    print("starting transmission loop")
    global running
    global transmit_delay_ms
    global lane_timings
    global node_comms
    global lane_configurations

    while running:
        lane_pointer = 0
        try:
            lane_timing_commands = []
            enabled_lanes = 0
            while(lane_pointer < len(lane_timings)):
                if(lane_configurations[lane_pointer].is_enabled):
                    last_pass = lane_timings[lane_pointer][1] + ((lane_timings[lane_pointer][2] - lane_timings[lane_pointer][1]) // 2)
                    command = commands.LaneTimings(lane_timings[lane_pointer][0],last_pass,lane_timings[lane_pointer][4])
                    lane_timing_commands.append(command)
                    enabled_lanes |= 1 << lane_pointer
                lane_pointer += 1
            
            command = commands.NodeTimings(time.ticks_ms(), len(lane_timings), enabled_lanes, lane_timing_commands)
            node_comms.send_command(command)
            await asyncio.sleep_ms(transmit_delay_ms)
        except Exception as e:
            print("transmit error: ",str(e))

async def rssi_loop():
    print("starting rssi loop")
    global running
    global transmit_delay_ms
    global lane_timings
    global rssi_modules

    lane_last_rssi_read = []
    for i in range(len(rssi_modules)):
        lane_last_rssi_read.append(0)

    while running:
        try:
            lane_pointer = 0
            while(lane_pointer < len(lane_timings)):
                current_time = time.ticks_ms()
                current_rssi = rssi_modules[lane_pointer].read_value()
                lane_last_rssi_read[lane_pointer] = current_time

                last_pass_start = lane_timings[lane_pointer][1]
                last_pass_end = lane_timings[lane_pointer][2]
                pass_count = lane_timings[lane_pointer][4]
                
                pass_state = peak_detectors[lane_pointer].add_reading(current_rssi,current_time)
                if(pass_state == 1):
                    last_pass_start = current_time
                    print("Lane "+ str(lane_pointer) +"Pass Start")
                elif(pass_state == 3):
                    print("Lane "+ str(lane_pointer) +"Pass End")
                    last_pass_end = current_time
                    pass_count += 1
                
                lane_timings[lane_pointer] = (current_rssi, last_pass_start, last_pass_end, pass_state, pass_count)
                lane_pointer += 1
            
            min_delay = calculate_minimum_delay(lane_last_rssi_read, polling_delay_ms)
            await asyncio.sleep_ms(min_delay)
        except Exception as e:
            print("rssi error: ",str(e))

async def init_device():
    print("Running startup")

    global radio

    print("Tuning Modules")

    for i in range(len(rx_modules)):
        frequency = lane_configurations[i].frequency_in_mhz
        success = rx_modules[i].tune(frequency)
        print("Module: ", str(i), "Tuned To: ", frequency, "MHz Success: ", success)

    asyncio.create_task(radio.start_tcp_server())
    asyncio.create_task(transmission_loop())
    asyncio.create_task(rssi_loop())

    await command_loop();

lan=network.LAN(mdc=machine.Pin(31), mdio=machine.Pin(52),
    phy_type=network.PHY_IP101, phy_addr=1, reset=machine.Pin(51),
    ref_clk_mode=machine.Pin.IN, ref_clk=machine.Pin(50))
lan.active(True)

while(not lan.isconnected()):
    time.sleep(1)
    print("Waiting for LAN connection...")
print("Lan Connected:",lan.ipconfig("addr4")[0])

radio = comms.TCPServerSocketRadio()
node_comms = comms.NodeCommunication(radio)
running = True
node_id = 1
transmit_delay_ms = 100 #10hz
polling_delay_ms = 10 #100hz
filter_cutoff_frequency = 50


RECEIVER_SCLK_PIN = 15
RECEIVER_MOSI_PIN = 3

print("Initializing Devices")

lane_configurations = [
    LaneConfiguration(True,4,5658,18000,18000),
    LaneConfiguration(True,4,5695,18000,18000),
    LaneConfiguration(True,4,5732,18000,18000),
    LaneConfiguration(True,4,5769,18000,18000),
    LaneConfiguration(True,4,5806,18000,18000),
    LaneConfiguration(True,4,5843,18000,18000),
    LaneConfiguration(True,4,5880,18000,18000),
    LaneConfiguration(True,4,5917,18000,18000),
]

rssi_modules = [
    ADCReader(19, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(18, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(17, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(16, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(20, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(21, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(22, polling_delay_ms,filter_cutoff_frequency),
    ADCReader(23, polling_delay_ms,filter_cutoff_frequency),
]
rx_modules = [
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,14),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,6),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,5),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,4),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,27),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,32),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,33),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,26)
]
peak_detectors = [
    PeakDetector(lane_configurations[0].entry_threshold, lane_configurations[0].exit_threshold),
    PeakDetector(lane_configurations[1].entry_threshold, lane_configurations[1].exit_threshold),
    PeakDetector(lane_configurations[2].entry_threshold, lane_configurations[2].exit_threshold),
    PeakDetector(lane_configurations[3].entry_threshold, lane_configurations[3].exit_threshold),
    PeakDetector(lane_configurations[4].entry_threshold, lane_configurations[4].exit_threshold),
    PeakDetector(lane_configurations[5].entry_threshold, lane_configurations[5].exit_threshold),
    PeakDetector(lane_configurations[6].entry_threshold, lane_configurations[6].exit_threshold),
    PeakDetector(lane_configurations[7].entry_threshold, lane_configurations[7].exit_threshold)
]


print("Devices Initialized")

#rssi, last_pass_start,last_pass_end,pass_state, pass count
lane_timings = []
for i in range(len(rx_modules)):
    lane_timings.append((0,0,0,0,0))

asyncio.run(init_device())
