import json

LANE_CONFIG_FILE = '/lane_config.json'
NETWORK_CONFIG_FILE = '/network_config.json'


def load_lane_config():
    """Load persisted lane configurations from flash.
    Returns a list of dicts or None if no saved config exists."""
    try:
        with open(LANE_CONFIG_FILE, 'r') as f:
            return json.load(f)
    except Exception:
        return None


def save_lane_config(lane_configurations):
    """Persist lane configurations to flash as JSON."""
    try:
        data = []
        for lc in lane_configurations:
            data.append({
                'is_enabled': lc.is_enabled,
                'bandId': lc.bandId,
                'frequency_in_mhz': lc.frequency_in_mhz,
                'entry_threshold': lc.entry_threshold,
                'exit_threshold': lc.exit_threshold,
            })
        with open(LANE_CONFIG_FILE, 'w') as f:
            json.dump(data, f)
        return True
    except Exception as e:
        print("Failed to save lane config:", e)
        return False


def load_network_config():
    """Load persisted network configuration from flash.
    Returns a dict with keys dhcp, ip, subnet, gateway, dns,
    or None if no saved config exists."""
    try:
        with open(NETWORK_CONFIG_FILE, 'r') as f:
            return json.load(f)
    except Exception:
        return None


def save_network_config(dhcp, ip, subnet, gateway, dns='8.8.8.8'):
    """Persist network configuration to flash as JSON."""
    try:
        data = {
            'dhcp': dhcp,
            'ip': ip,
            'subnet': subnet,
            'gateway': gateway,
            'dns': dns,
        }
        with open(NETWORK_CONFIG_FILE, 'w') as f:
            json.dump(data, f)
        return True
    except Exception as e:
        print("Failed to save network config:", e)
        return False
