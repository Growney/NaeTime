import network
import time
import asyncio
import commands as commands
import config
from devices.rssi import ADCReader
from devices.rssi import PeakDetector
from devices.rx5808 import Rx5808RegisterCommunication
from node import LaneConfiguration
import machine
import comms
from collections import deque # type: ignore
from esp32 import LDO

class LaneTiming:
    def __init__(self,rssi_read_time,rssi, last_pass_start,last_pass_end, pass_state, pass_count):
        self._rssi_read_time = rssi_read_time
        self._rssi = rssi
        self._last_pass_start = last_pass_start
        self._last_pass_end = last_pass_end
        self._pass_state = pass_state
        self._pass_count = pass_count
    
    @property
    def rssi_read_time(self):
        return self._rssi_read_time
    
    @rssi_read_time.setter
    def rssi_read_time(self, value):
        self._rssi_read_time = value

    @property
    def rssi(self):
        return self._rssi

    @rssi.setter
    def rssi(self, value):
        self._rssi = value

    @property
    def last_pass_start(self):
        return self._last_pass_start

    @last_pass_start.setter
    def last_pass_start(self, value):
        self._last_pass_start = value

    @property
    def last_pass_end(self):
        return self._last_pass_end
    
    @last_pass_end.setter
    def last_pass_end(self, value):
        self._last_pass_end = value
    
    @property
    def pass_state(self):
        return self._pass_state

    @pass_state.setter
    def pass_state(self, value):
        self._pass_state = value

    @property
    def pass_count(self):
        return self._pass_count
    
    @pass_count.setter
    def pass_count(self, value):
        self._pass_count = value
    
class Pass:
    def __init__(self, lane,pass_count, start_time, end_time):
        self._lane = lane
        self._pass_count = pass_count
        self._start_time = start_time
        self._end_time = end_time
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def pass_count(self):
        return self._pass_count
    
    @property
    def start_time(self):
        return self._start_time
    
    @property
    def end_time(self):
        return self._end_time

def frequency_to_delay_ms(frequency_hz):
    if frequency_hz <= 0:
        raise ValueError("Frequency must be greater than 0")
    return 1000 / frequency_hz

async def command_loop(comms_obj):
    """Unified command loop. Handles every command type for any radio/comms object.
    Run one task per transport (TCP, UART) passing the respective comms instance."""
    print("starting command loop for", comms_obj)
    global running
    global node_id
    global transmit_delay_ms
    global polling_delay_ms
    global rx_modules
    global lane_configurations
    global lan

    while running:
        command = await comms_obj.wait_for_command()
        try:
            if command is None:
                print("No command received")
                continue
            elif isinstance(command, commands.TuneLane):
                print("Tune command received Lane: "+str(command.lane)+" Frequency: "+str(command.frequency_in_mhz )+" MHz Band: "+str(command.bandId))
                if(command.lane < len(rx_modules) and command.lane >= 0 and rx_modules[command.lane].tune(command.frequency_in_mhz)):
                    lane_configurations[command.lane].bandId = command.bandId;
                    lane_configurations[command.lane].frequency_in_mhz = command.frequency_in_mhz
                    config.save_lane_config(lane_configurations)
                    print("Tune Successful")
                    comms_obj.send_ack_for_command(command)
                else:
                    comms_obj.send_error_for_command(command)
                    print("Tune Failed")
            elif isinstance(command, commands.ConfigureLaneEntryThreshold):
                print("Configure Entry Threshold Lane: "+str(command.lane)+" Threshold: "+str(command.entry_threshold))
                if(command.lane < len(peak_detectors) and command.lane >= 0):
                    peak_detectors[command.lane].entry_threshold = command.entry_threshold
                    lane_configurations[command.lane].entry_threshold = command.entry_threshold
                    config.save_lane_config(lane_configurations)
                    print("Entry Threshold Set")
                    comms_obj.send_ack_for_command(command)
                else:
                    comms_obj.send_error_for_command(command)
            elif isinstance(command, commands.ConfigureLaneExitThreshold):
                print("Configure Exit Threshold Lane: "+str(command.lane)+" Threshold: "+str(command.exit_threshold))
                if(command.lane < len(peak_detectors) and command.lane >= 0):
                    peak_detectors[command.lane].exit_threshold = command.exit_threshold
                    lane_configurations[command.lane].exit_threshold = command.exit_threshold
                    config.save_lane_config(lane_configurations)
                    print("Exit Threshold Set")
                    comms_obj.send_ack_for_command(command)
                else:
                    comms_obj.send_error_for_command(command)
            elif isinstance(command, commands.ConfigureLaneEnabled):
                print("Configure Lane Enabled Lane: "+str(command.lane)+" Enabled: "+str(command.enabled))
                if(command.lane < len(lane_configurations) and command.lane >= 0):
                    lane_configurations[command.lane].is_enabled = command.enabled == 1
                    config.save_lane_config(lane_configurations)
                    print("Lane Enabled Set")
                    comms_obj.send_ack_for_command(command)
                else:
                    comms_obj.send_error_for_command(command)
            elif isinstance(command, commands.RequestLaneConfigurations):
                print("Lane Configuration Request Received")
                lane_mask = command.lanes[0] if command.lanes else 0xFF
                response_configs = []
                enabled_lanes = 0
                for lane_index in range(len(lane_configurations)):
                    if lane_mask & (1 << lane_index):
                        
                        hardware_frequency = rx_modules[lane_index].get_stored_frequency()
                        transmission_frequency = lane_configurations[lane_index].frequency_in_mhz
                        
                        if(not Rx5808RegisterCommunication.confirm_matched_register_frequencies(lane_configurations[lane_index].frequency_in_mhz, hardware_frequency)):
                            transmission_frequency = hardware_frequency

                        response_configs.append(commands.LaneConfiguration(
                            lane_configurations[lane_index].bandId,
                            transmission_frequency,
                            lane_configurations[lane_index].entry_threshold,
                            lane_configurations[lane_index].exit_threshold))
                    if lane_configurations[lane_index].is_enabled:
                        enabled_lanes |= 1 << lane_index
                response_command = commands.LaneConfigurationsResponse(enabled_lanes, response_configs)
                comms_obj.send_response_for_command(response_command)
            elif isinstance(command, commands.ConfigureNode):
                print("Configure command received")
                node_id = command.node_id
                transmit_delay_ms = frequency_to_delay_ms(command.transmit_frequency_hz)
                polling_delay_ms = frequency_to_delay_ms(command.polling_frequency_hz)
            elif isinstance(command, commands.QueryNetworkStatus):
                connected = lan.isconnected()
                dhcp = lan.ipconfig('dhcp4')
                ifconfig = lan.ifconfig()
                ip = ifconfig[0]
                subnet = ifconfig[1]
                gateway = ifconfig[2]
                print("query: network status connected=", connected, "dhcp=", dhcp, "ip=", ip)
                comms_obj.send_response_for_command(
                    commands.NetworkStatusResponse(connected, dhcp, ip, subnet, gateway))
            elif isinstance(command, commands.ConfigureNetwork):
                print("command: configure network dhcp=", command.dhcp, "ip=", command.ip)
                try:
                    if command.dhcp:
                        lan.ipconfig(dhcp4=True)
                        dns = lan.ifconfig()[3]
                    else:
                        lan.ipconfig(dhcp4=False)
                        dns = lan.ifconfig()[3]
                        lan.ifconfig((command.ip, command.subnet, command.gateway, dns))
                    config.save_network_config(command.dhcp, command.ip, command.subnet, command.gateway, dns)
                    print("Network configuration applied and saved")
                    comms_obj.send_ack_for_command(command)
                except Exception as e:
                    print("Failed to apply network configuration:", e)
                    comms_obj.send_error_for_command(command)
            else:
                print("Unknown command received", command)
        except Exception as e:
            print("command error: ",str(e))

async def transmission_loop():
    print("starting transmission loop")
    global running
    global transmit_delay_ms
    global lane_timings
    global node_comms
    global lane_configurations
    global pass_queue
    global lan

    total_time_us = 0
    previous_time_us = time.ticks_us()
    while running:
        if not lan.isconnected():
            await asyncio.sleep_ms(1000)
            print("Waiting for LAN connection...")
            continue
        lane_pointer = 0
        try:
            lane_timing_commands = []
            enabled_lanes = 0
            while(lane_pointer < len(lane_timings)):
                if(lane_configurations[lane_pointer].is_enabled):
                    current_timing = lane_timings[lane_pointer]
                    command = commands.LaneRssi(lane_pointer, current_timing.rssi_read_time, current_timing.rssi)
                    lane_timing_commands.append(command)
                    enabled_lanes |= 1 << lane_pointer
                lane_pointer += 1
            
            current_time_us = time.ticks_us()
            total_time_us += time.ticks_diff(current_time_us, previous_time_us)
            previous_time_us = current_time_us

            command = commands.NodeStatus(total_time_us, len(lane_timings), enabled_lanes, lane_timing_commands)
            node_comms.send_command(command)

            while(len(pass_queue) > 0):
                pass_event = pass_queue.popleft()
                print("Sending Pass Event Lane: "+str(pass_event.lane)+" Pass Count: "+str(pass_event.pass_count)+" Start Time: "+str(pass_event.start_time)+" End Time: "+str(pass_event.end_time))
                pass_command = commands.LanePassEvent(pass_event.lane, pass_event.pass_count, pass_event.start_time, pass_event.end_time)
                node_comms.send_command(pass_command)
                
                   
            await asyncio.sleep_ms(transmit_delay_ms)

        except Exception as e:
            print("transmit error: ",str(e))

async def rssi_loop():
    print("starting rssi loop")
    global running
    global transmit_delay_ms
    global lane_timings
    global rssi_modules
    global lane_configurations
    global pass_queue

    total_time_us = 0
    previous_time_us = time.ticks_us()
    while running:
        try:
            lane_pointer = 0
            loop_start = time.ticks_ms()
            while(lane_pointer < len(lane_timings)):

                if(not lane_configurations[lane_pointer].is_enabled):
                    lane_pointer += 1
                    continue

                current_time_us = time.ticks_us()
                total_time_us += time.ticks_diff(current_time_us, previous_time_us)
                previous_time_us = current_time_us

                current_rssi = rssi_modules[lane_pointer].read_value()

                current_timing = lane_timings[lane_pointer]
                current_timing.rssi_read_time = total_time_us
                current_timing.rssi = current_rssi
                pass_state = peak_detectors[lane_pointer].add_reading(current_rssi,total_time_us)
                if(pass_state == 1):
                    current_timing.last_pass_start = total_time_us
                elif(pass_state == 3):
                    current_timing.last_pass_end = total_time_us
                    current_timing.pass_count += 1
                    pass_queue.append(Pass(lane_pointer,current_timing.pass_count,current_timing.last_pass_start,current_timing.last_pass_end))
                        
                lane_pointer += 1
            loop_end = time.ticks_ms()
            elapsed = time.ticks_diff(loop_end, loop_start)
            
            calculated_delay = polling_delay_ms - elapsed
            if(calculated_delay > 0):
                await asyncio.sleep_ms(calculated_delay)
                
        except Exception as e:
            print("rssi error: ",str(e))

async def init_device():
    print("Running startup")

    global radio

    print("Tuning Modules")

    for i in range(len(rx_modules)):
        rx_modules[i].init()
    
    print("modules initialized")
    await asyncio.sleep_ms(1000)

    for i in range(len(rx_modules)):
        frequency = lane_configurations[i].frequency_in_mhz
        success = rx_modules[i].tune(frequency)
        print("Lane "+str(i)+" tuned to "+str(frequency)+" MHz"+" Success: "+str(success))

    asyncio.create_task(radio.start_tcp_server())
    asyncio.create_task(transmission_loop())
    asyncio.create_task(rssi_loop())
    asyncio.create_task(uart_radio.start())
    asyncio.create_task(command_loop(ui_comms))

    await command_loop(node_comms)
ldo4 = LDO(4, 3300) # LDO4 powers the RF modules, set to 3.3V
uartTx = machine.Pin(47, machine.Pin.OUT)
uartRx = machine.Pin(48, machine.Pin.IN, machine.Pin.PULL_DOWN)

comms_uart = machine.UART(3, baudrate=115200, tx=uartTx, rx=uartRx)
uart_radio = comms.UARTRadio(comms_uart)
ui_comms = comms.NodeCommunication(uart_radio)

lan=network.LAN(mdc=machine.Pin(31), mdio=machine.Pin(52),
    phy_type=network.PHY_IP101, phy_addr=1, reset=machine.Pin(51),
    ref_clk_mode=machine.Pin.IN, ref_clk=machine.Pin(50))
lan.active(True)
saved_net = config.load_network_config()
if saved_net is not None:
    print("Applying saved network configuration dhcp=", saved_net['dhcp'], "ip=", saved_net.get('ip', ''))
    if saved_net['dhcp']:
        lan.ipconfig(dhcp4=True)
    else:
        lan.ipconfig(dhcp4=False)
        lan.ifconfig((saved_net['ip'], saved_net['subnet'], saved_net['gateway'], saved_net.get('dns', '8.8.8.8')))
else:
    print("No saved network config, using defaults")
    lan.ipconfig(dhcp4=False)
    lan.ifconfig(('192.168.1.4', '255.255.255.0', '192.168.1.1', '8.8.8.8'))

radio = comms.TCPServerSocketRadio()
node_comms = comms.NodeCommunication(radio)
running = True
node_id = 1
transmit_delay_ms = 100 #10hz
polling_delay_ms = 100 #100hz
filter_cutoff_frequency = 200


pass_queue = deque([],500)

RECEIVER_SCLK_PIN = 15
RECEIVER_MOSI_PIN = 3

print("Initializing Devices")

lane_configurations = [
    LaneConfiguration(False,4,5658,60000,60000),
    LaneConfiguration(False,4,5695,60000,60000),
    LaneConfiguration(False,4,5732,60000,60000),
    LaneConfiguration(False,4,5769,60000,60000),
    LaneConfiguration(False,4,5806,60000,60000),
    LaneConfiguration(False,4,5843,60000,60000),
    LaneConfiguration(False,4,5880,60000,60000),
    LaneConfiguration(False,4,5917,60000,60000),
]
saved_lanes = config.load_lane_config()
if saved_lanes is not None:
    print("Applying saved lane configurations")
    for i, lc in enumerate(saved_lanes):
        if i < len(lane_configurations):
            lane_configurations[i].is_enabled = lc['is_enabled']
            lane_configurations[i].bandId = lc['bandId']
            lane_configurations[i].frequency_in_mhz = lc['frequency_in_mhz']
            lane_configurations[i].entry_threshold = lc['entry_threshold']
            lane_configurations[i].exit_threshold = lc['exit_threshold']
else:
    print("No saved lane config, using defaults")

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
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,26),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,27),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,32),
    Rx5808RegisterCommunication(RECEIVER_SCLK_PIN,RECEIVER_MOSI_PIN,33)
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
    lane_timings.append(LaneTiming(0,0,0,0,0,0))

asyncio.run(init_device())
