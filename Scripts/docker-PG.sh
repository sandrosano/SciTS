cd /sdPG
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir pg_config pg_log pg_data

docker run -d  --cap-add=IPC_LOCK  --cap-add=NET_ADMIN --cap-add=SYS_NICE -p 5433:5432 \
        \
        -v /sdPG/pg_config):/etc/postgresql/ \
        -v /sdPG/pg_log):/var/log/postgresql/ \
        -v /sdPG/pg_data):/var/lib/postgresql/data/ \
        -e POSTGRES_PASSWORD=PostgresPW \
         postgres -c shared_buffers=256MB -c max_connections=200
