FROM python:3.9-slim

ENV PYTHONUNBUFFERED=1

WORKDIR /app

# Install required packages - pin to a specific version that works
RUN pip install --no-cache-dir "paho-mqtt>=2.0.0"

# Copy the script
COPY IoTSimulator/main.py main.py

# Run the script
CMD ["python", "main.py"]