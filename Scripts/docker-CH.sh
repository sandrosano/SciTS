cd /sdCH
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir ch_log ch_config ch_data


docker run -d  --cap-add=IPC_LOCK  --cap-add=NET_ADMIN --cap-add=SYS_NICE -p 8123:8123 -p 9000:9000 -p 9009:9009   \
    -v /sdCH/ch_data):/var/lib/clickhouse/ \
        -v /sdCH/ch_log):/var/log/clickhouse-server/ \
        -v /sdCH/ch_config):/etc/clickhouse-server/ \
         --ulimit nofile=262144:262144 \
        clickhouse/clickhouse-server
 

 docker run -d --network=host --name some-clickhouse-server \
  -v "/sdCH/ch_data:/var/lib/clickhouse/" \
    -v "/sdCH/ch_logs:/var/log/clickhouse-server/" \
     --ulimit nofile=262144:262144 clickhouse