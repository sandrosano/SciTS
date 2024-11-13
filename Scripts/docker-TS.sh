cd /sdTS
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir ts_data ts_log ts_config 

docker run -d --network=host --cap-add=IPC_LOCK  --cap-add=NET_ADMIN --cap-add=SYS_NICE  -p 6432:5432 \
         \
        -v /sdTS/ts_data):/home/postgres/pgdata/data/ \
        -v /sdTS/ts_config):/var/lib/postgresql/data/  \
        -v /sdTS/ts_log):/var/log/postrgresql/               \
        -e POSTGRES_PASSWORD=TimescalePW     \
        timescale/timescaledb:latest-pg14