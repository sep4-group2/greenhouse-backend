import paho.mqtt.client as mqtt
import json
import time
import random
import datetime
import uuid
import os

# MQTT Configuration
BROKER_HOST = os.getenv("MQTT_HOST", "localhost")  # Use service name in Docker network
BROKER_PORT = int(os.getenv("MQTT_PORT", "1883"))
CLIENT_ID = f'greenhouse-simulator-{uuid.uuid4().hex[:8]}'
MAC_ADDRESS = "FF:9A:4C:98:6E:17"

# Topics
SENSOR_TOPIC = "greenhouse/sensor"
ACTION_TOPIC = "greenhouse/action"

# Define possible actions
ACTIONS = ["Watering", "Fertilization", "Lighting"]

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"Connected to MQTT Broker at {BROKER_HOST}:{BROKER_PORT}")
    else:
        print(f"Failed to connect, return code {rc}")

def generate_timestamp():
    """Generate current timestamp in ISO format"""
    return datetime.datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")

def generate_sensor_data():
    """Generate random sensor data for temperature and humidity"""
    timestamp = generate_timestamp()
    
    # Generate random values within realistic ranges
    temperature = round(random.uniform(18.0, 32.0), 1)
    humidity = round(random.uniform(40.0, 80.0), 1)
    soil_humidity = round(random.uniform(30.0, 70.0), 1)
    air_humidity = round(random.uniform(30.0, 70.0), 1)
    
    payload = {
        "MacAddress": MAC_ADDRESS,
        "SensorData": [
            {
                "Type": "Temperature",
                "Value": temperature,
                "Unit": "C",
            },
            {
                "Type": "Humidity",
                "Value": humidity,
                "Unit": "%",
            },
            {
                "Type": "SoilHumidity",
                "Value": soil_humidity,
                "Unit": "%",
            },
            {
                "Type": "AirHumidity",
                "Value": air_humidity,
                "Unit": "%",
            }
        ]
    }
    
    return json.dumps(payload)

def generate_action_data():
    """Generate random action data"""
    command = random.choice(ACTIONS)
    status = random.choice([True, False])
    
    payload = {
        "MacAddress": MAC_ADDRESS,
        "Command": command,
        "Status": status,
        "Timestamp": generate_timestamp()
    }
    
    return json.dumps(payload)

def main():
    # Initialize MQTT client
    client = None

    # Set wait time from environment variable or default to 60 seconds (1 minute)
    wait_time = int(os.getenv("SENSOR_INTERVAL", "60"))

    try:
        # Use MQTT v5 protocol explicitly
        client = mqtt.Client(client_id=CLIENT_ID, protocol=mqtt.MQTTv5, callback_api_version=mqtt.CallbackAPIVersion.VERSION2)

        # Modern callback registration for MQTT v5
        def on_connect(client, userdata, flags, reasonCode, properties):
            print(f"on_connect called with reasonCode={reasonCode}")
            if reasonCode == 0:
                print(f"Connected to MQTT Broker at {BROKER_HOST}:{BROKER_PORT}")
            else:
                print(f"Failed to connect, reasonCode {reasonCode}")

        client.on_connect = on_connect

        # Connect to broker
        print(f"Connecting to {BROKER_HOST}:{BROKER_PORT} ...")
        client.connect(BROKER_HOST, BROKER_PORT, 60)
        client.loop_start()

        # Wait to ensure connection is established
        time.sleep(2)

        while True:
            # Publish sensor data
            sensor_payload = generate_sensor_data()
            result = client.publish(SENSOR_TOPIC, sensor_payload)
            if result.rc == mqtt.MQTT_ERR_SUCCESS:
                print(f"Sent sensor data to {SENSOR_TOPIC}: {sensor_payload}")
            else:
                print(f"Failed to send sensor data to {SENSOR_TOPIC}, rc={result.rc}")

            # Occasionally publish action data (roughly every 2-5 minutes)
            if random.random() < 0.3:  # 30% chance each cycle
                action_payload = generate_action_data()
                result = client.publish(ACTION_TOPIC, action_payload)
                if result.rc == mqtt.MQTT_ERR_SUCCESS:
                    print(f"Sent action data to {ACTION_TOPIC}: {action_payload}")
                else:
                    print(f"Failed to send action data to {ACTION_TOPIC}, rc={result.rc}")

            # Wait for the specified interval before next sensor update
            print(f"Waiting {wait_time} seconds until next update...")
            time.sleep(wait_time)

    except KeyboardInterrupt:
        print("Script terminated by user")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        # Clean up - make sure client exists before stopping/disconnecting
        if client:
            try:
                client.loop_stop()
                client.disconnect()
                print("Disconnected from MQTT broker")
            except Exception as e:
                print(f"Error during disconnect: {e}")

if __name__ == "__main__":
    main()