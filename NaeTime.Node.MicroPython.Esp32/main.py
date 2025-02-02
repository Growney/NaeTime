import time
import asyncio
import commands
from comms import RFM69NodeCommunication
from devices.rssi import ADCReader
from devices.rssi import PeakDetector
from devices.rx5808 import Rx5808RegisterCommunication
import machine

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

    while running:
        command = await node_comms.wait_for_command()
        try:
            if isinstance(command, commands.TuneLane):
                print("Tune command received Lane: "+str(command.lane)+" Frequency: "+str(command.frequency_in_mhz))
                if(command.lane < len(rx_modules) and command.lane >= 0 and rx_modules[command.lane].tune(command.frequency_in_mhz)):
                    print("Tune Successful")
                    asyncio.create_task(sound_buzzer(((1,100),(0,100),(1,100))))
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
                    print("Tune Failed")
            elif isinstance(command, commands.ConfigureLaneEntryThreshold):
                print("Configure Entry Threshold Lane: "+str(command.lane)+" Threshold: "+str(command.entry_threshold))
                if(command.lane < len(peak_detectors) and command.lane >= 0):
                    peak_detectors[command.lane].entry_threshold = command.entry_threshold
                    print("Entry Threshold Set")
                    asyncio.create_task(sound_buzzer(((1,100),(0,100),(1,100),(0,100),(1,100))))
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
            elif isinstance(command, commands.ConfigureLaneExitThreshold):
                print("Configure Exit Threshold Lane: "+str(command.lane)+" Threshold: "+str(command.exit_threshold))
                if(command.lane < len(peak_detectors) and command.lane >= 0):
                    peak_detectors[command.lane].exit_threshold = command.exit_threshold
                    print("Exit Threshold Set")
                    asyncio.create_task(sound_buzzer(((1,100),(0,100),(1,100),(0,100),(1,100))))
                    node_comms.send_ack_for_command(command)
                else:
                    node_comms.send_error_for_command(command)
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

    while running:
        delay = int(transmit_delay_ms / len(lane_timings))
        lane_pointer = 0
        try:
            while(lane_pointer < len(lane_timings)):
                current_time = time.ticks_ms()
                command = commands.LaneTimings(current_time, lane_pointer, lane_timings[lane_pointer][0],lane_timings[lane_pointer][1],lane_timings[lane_pointer][2],lane_timings[lane_pointer][3],lane_timings[lane_pointer][4])
                node_comms.send_command(command)
                await asyncio.sleep_ms(delay)
                lane_pointer += 1
        except Exception as e:
            print("transmit error: ",str(e))

async def sound_buzzer(pattern):
    global buzzer_pin
    for step in pattern:

        buzzer_pin.value(step[0])
        await asyncio.sleep_ms(step[1])
    
    buzzer_pin.value(0)


async def rssi_loop():
    print("starting rssi loop")
    global running
    global transmit_delay_ms
    global lane_timings
    global rssi_modules

    lane_last_rssi_read = [0,0,0]
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
    await sound_buzzer([(1,500)]);

    rx_modules[0].tune(5658)
    rx_modules[0].tune(5695)
    rx_modules[0].tune(5732)

    asyncio.create_task(transmission_loop())
    asyncio.create_task(rssi_loop())

    await sound_buzzer([(1,200),(0,200),(1,200)]);
    await command_loop();



print("Initializing Devices")
CS = 21
RESET = 18
DIO0 = 17
SCK = 48
MOSI = 38
MISO = 47
BUZZER = 10
RADIO_FREQ_MHZ = 433.0

running = True
node_id = 1
transmit_delay_ms = 100 #10hz
polling_delay_ms = 10 #100hz
filter_cutoff_frequency = 20

buzzer_pin = machine.Pin(BUZZER, machine.Pin.OUT)
node_comms = RFM69NodeCommunication(CS, RESET, DIO0, SCK, MOSI, MISO, RADIO_FREQ_MHZ, "NaeTime")
rssi_modules = [
ADCReader(1, polling_delay_ms,filter_cutoff_frequency),
ADCReader(2, polling_delay_ms,filter_cutoff_frequency),
ADCReader(4, polling_delay_ms,filter_cutoff_frequency),
]
rx_modules = [
    Rx5808RegisterCommunication(6,5,9),
    Rx5808RegisterCommunication(6,5,8),
    Rx5808RegisterCommunication(6,5,7)
]
peak_detectors = [
    PeakDetector(40000,40000),
    PeakDetector(40000,40000),
    PeakDetector(40000,40000)
]

print("Devices Initialized")
#rssi, last_pass_start,last_pass_end,pass_state, pass count
lane_timings = [(0,0,0,0,0),(0,0,0,0,0),(0,0,0,0,0)]

asyncio.run(init_device())


