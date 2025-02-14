cd /sdIF
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir if_data if_config if_log 

docker run -d  --cap-add=IPC_LOCK  --cap-add=NET_ADMIN --cap-add=SYS_NICE  -p 8087:8086 \
      -v /sdIF/if_data:/var/lib/influxdb2/ \
      -v /sdIF/if_config:/etc/influxdb2/ \
        -v /sdIF/if_log:/var/log/influxdb2/  \
        -e DOCKER_INFLUXDB_INIT_MODE=setup \
      -e DOCKER_INFLUXDB_INIT_USERNAME=scits \
      -e DOCKER_INFLUXDB_INIT_PASSWORD=InfluxPW \
      -e DOCKER_INFLUXDB_INIT_ORG=scits \
      -e DOCKER_INFLUXDB_INIT_BUCKET=scitsdb \
      -e DOCKER_INFLUXDB_INIT_ADMIN_TOKEN=u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
      influxdb:2.0


      # or with: --network=host