# RUN 2024 - Init

## on client 
apt install -y wget curl gpg sudo htop vim  git glances net-tools fio sysbench   iperf 
snap install dotnet-sdk --classic --channel 8.0/stable
git clone https://github.com/sandrosano/SciTS.git


## on server

sudo su # From now on everything as root - and: yes, sudo is unneccessary after this line
apt update -y
apt install -y tzdata
apt upgrade -y
apt install -y wget curl gpg sudo htop vim pip git glances net-tools fio sysbench glances iperf


## Format disk
mkfs.xfs -f /dev/nvme0n2
mkdir /store
mount -t xfs /dev/nvme0n2 /store
mkdir /store/CH /store/TS /store/IF /store/FTP


# Configure DBMS
## Influx
curl --silent --location -O \
https://repos.influxdata.com/influxdata-archive.key
echo "943666881a1b8d9b849b74caebf02d3465d6beb716510d86a39f6c8e8dac7515  influxdata-archive.key" \
| sha256sum --check - && cat influxdata-archive.key \
| gpg --dearmor \
| tee /etc/apt/trusted.gpg.d/influxdata-archive.gpg > /dev/null \
&& echo 'deb [signed-by=/etc/apt/trusted.gpg.d/influxdata-archive.gpg] https://repos.influxdata.com/debian stable main' \
| tee /etc/apt/sources.list.d/influxdata.list
apt-get update && apt-get install influxdb2
service influxdb start

influx setup -n scitsconfig\
  --org scits \
  --bucket scitsdb \
  --username scits \
  --password InfluxPW \
  --host http://127.0.0.1:8086 \
  --token u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
 --force  

influx config create --active \
  -n scitsconfigclient \
  -u http://127.0.0.1:8086 \
  -t u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
  -o scits     

export INFLUXD_HTTP_BIND_ADDRESS=127.0.0.1:8086
# set config fro storage write timeout increase
echo "storage-write-timeout = 60" >> /etc/influxdb/config.toml

# curl http://127.0.0.1:8086 | cat 


##ClickHouse
  apt-get install -y apt-transport-https ca-certificates curl gnupg
curl -fsSL 'https://packages.clickhouse.com/rpm/lts/repodata/repomd.xml.key' |   gpg --dearmor -o /usr/share/keyrings/clickhouse-keyring.gpg

echo "deb [signed-by=/usr/share/keyrings/clickhouse-keyring.gpg] https://packages.clickhouse.com/deb stable main" |   tee \
    /etc/apt/sources.list.d/clickhouse.list
  apt-get update
  apt-get install -y clickhouse-server clickhouse-client
#  set password: ClickhousePWscits
echo " <listen_host>::</listen_host> " >> /etc/clickhouse-server/config.xml
  service clickhouse-server start

## Timescale

  apt install gnupg postgresql-common apt-transport-https lsb-release wget
  sh  /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh
curl -s https://packagecloud.io/install/repositories/timescale/timescaledb/script.deb.sh | bash
  apt update
  apt install timescaledb-2-postgresql-17  postgresql-client-17  
psql -U postgres
# WICHTIG! fist set password, then change to PG_HBA_CONF
# set pw: TimescalePWscitskit  :
\password
#  hier alle "peer" auf "scram-sha-256" :
vim  /etc/postgresql/17/main/pg_hba.conf 
# # , my file looks like this:
# # Database administrative login by Unix domain socket
# local   all             postgres                                scram-sha-256

# # TYPE  DATABASE        USER            ADDRESS                 METHOD

# # "local" is for Unix domain socket connections only
# local   all             all                                     scram-sha-256
# # IPv4 local connections:
# host    all             all             127.0.0.1/32            scram-sha-256
# # IPv6 local connections:
# host    all             all             ::1/128                 scram-sha-256
# # Allow replication connections from localhost, by a user with the
# # replication privilege.
# local   replication     all                                     scram-sha-256
# host    replication     all             127.0.0.1/32            scram-sha-256
# host    replication     all             ::1/128                 scram-sha-256


apt install pgbouncer

vim /etc/pgbouncer/pgbouncer.ini
# # overwrite pgbouncer.ini   with: 
# [databases]
# * = port=5432 host=/var/run/postgresql auth_user=postgres
# [pgbouncer]
# logfile = /var/log/pgbouncer/pgbouncer.log
# pidfile = /var/run/pgbouncer/pgbouncer.pid
# listen_addr = * 
# listen_port = 6432
# auth_type = md5
# auth_file = /etc/pgbouncer/userlist.txt
# #auth_hba_file = /etc/postgresql/17/main/pg_hba.conf 
# admin_users = postgres
# stats_users = postgres
# pool_mode = session
# ignore_startup_parameters = extra_float_digits
# max_client_conn = 1200
# default_pool_size = 50
# reserve_pool_size = 25
# reserve_pool_timeout = 3
# server_lifetime = 300
# server_idle_timeout = 120
# server_connect_timeout = 5
# server_login_retry = 1
# query_timeout = 180
# query_wait_timeout = 180
# client_idle_timeout = 180
# client_login_timeout = 180
# unix_socket_dir = /var/run/postgresql 

mkdir /var/log/pgbouncer
chmod 666 -R /var/log/pgbouncer/
mkdir /var/run/pgbouncer
chmod 666 -R /var/run/pgbouncer/
chown postgres:postgres /etc/pgbouncer/ -R

vim /usr/lib/systemd/system/pgbouncer.service
# # overwrite content in pgbouncer.service with: 
# [Unit]
# Description=A lightweight connection pooler for PostgreSQL
# Documentation=man:pgbouncer(1)
# After=syslog.target network.target
# [Service]
# RemainAfterExit=yes
# User=postgres
# Group=postgres
# # Path to the init file
# Environment=BOUNCERCONF=/etc/pgbouncer/pgbouncer.ini
# ExecStart=/usr/bin/pgbouncer -q /etc/pgbouncer/pgbouncer.ini
# ExecReload=/usr/bin/pgbouncer -R -q /etc/pgbouncer/pgbouncer.ini
# # Give a reasonable amount of time for the server to start up/shut down
# TimeoutSec=300
# [Install]
# WantedBy=multi-user.target

vim /etc/pgbouncer/userlist.txt
# # overwrite userlist.txt  with SHA extracted after login to postgres with:
psql -U postgres -p 5432
# then,      query: 
select passwd from pg_shadow;
# take that hash and insert it in userlist.txt
systemctl daemon-reload
service pgbouncer restart
service postgresql restart
# test connection
psql -U postgres -p 6432









##FTP
# Add Docker's official GPG key:
sudo apt-get update
sudo apt-get install ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
mkdir /store/FTP
sudo docker run     --detach        --env FTP_USER=ftpscits    --env FTP_PASS=FtpPWscitskit       --name my-ftp-server    --publish 20-21:20-21/tcp       --publish 40000-40999:40000-40999/tcp   --volume /store/FTP:/home/scits       garethflowers/ftp-server  
# change port config
docker exec -it my-ftp-server sh
# replace  with : pasv_max_port=40099 
vi /etc/vsftpd/vsftpd.conf



# Config data dir on Drive
cp -pvr /var/lib/clickhouse/ /store/
cp -pvr /var/lib/influxdb/ /store/
cp -pvr /var/lib/postgresql/ /store/
# change dir from /var/lib/... to /store/...
vim /etc/postgresql/17/main/postgresql.conf 
vim /etc/influxdb/config.toml 
vim /etc/clickhouse-server/config.xml 
# change args to : -w --disable-webui
vim /lib/systemd/system/glances.service
systemctl daemon-reload
service glances start

## exechute machine Benchmarks : CLIENT
# first run iperf -s on server and adapt ip below
### Infos
lscpu  | tee -a HWinfoC.log && 
lsmem  | tee -a HWinfoC.log && 
lshw  | tee -a HWinfoC.log && 
### CPU
#### Single Thread
sysbench --test=cpu --cpu-max-prime=20000 run --num-threads=1  | tee -a HWinfoC.log && 
#### MultiThread on Max Amount of Cores
sysbench --test=cpu --cpu-max-prime=20000 run --num-threads=64  | tee -a HWinfoC.log && 
### Memory
#### Single Thread
sysbench memory --memory-block-size=1G --memory-total-size=20G --memory-oper=write --threads=1 run  | tee -a HWinfoC.log  &&
#### MultiThread on Max Amounts of Cores
sysbench memory --memory-block-size=1G --memory-total-size=20G --memory-oper=write --threads=64 run   | tee -a HWinfoC.log && 
### Network, on clientside, with "iperf -s" running  on serverside
iperf -c 10.0.0.7  | tee -a HWinfoC.log  

 
## exechute machine Benchmarks : SERVER
# first run iperf -s on server and adapt ip below
### Infos
lscpu  | tee -a HWinfoC.log && 
lsmem  | tee -a HWinfoC.log && 
lshw  | tee -a HWinfoC.log && 
### CPU
#### Single Thread
sysbench --test=cpu --cpu-max-prime=20000 run --num-threads=1  | tee -a HWinfoC.log && 
#### MultiThread on Max Amount of Cores
sysbench --test=cpu --cpu-max-prime=20000 run --num-threads=8  | tee -a HWinfoC.log && 
### Memory
#### Single Thread
sysbench memory --memory-block-size=1G --memory-total-size=20G --memory-oper=write --threads=1 run  | tee -a HWinfoC.log  &&
#### MultiThread on Max Amounts of Cores
sysbench memory --memory-block-size=1G --memory-total-size=20G --memory-oper=write --threads=8 run   | tee -a HWinfoC.log && 
### Network, on clientside, with "iperf -s" running  on serverside
 

### Disk -numjobs=8
#### Write IOPS
fio --name=write_iops_test   -numjobs=8 \
  --filename=/store/fiotest/testwi --filesize=25G \
  --time_based --ramp_time=2s --runtime=1m \
  --ioengine=libaio --direct=1 --verify=0 --randrepeat=0 \
  --bs=4K --iodepth=256 --rw=randwrite \
  --iodepth_batch_submit=256  --iodepth_batch_complete_max=256 | tee -a Diskinfo8.log &&
#### Write Throughput
fio --name=write_bandwidth_test  -numjobs=8\
  --filename=/store/fiotest/testwt --filesize=25G \
  --time_based --ramp_time=2s --runtime=1m \
  --ioengine=libaio --direct=1 --verify=0 --randrepeat=0 \
  --numjobs=4 --thread --offset_increment=500G \
  --bs=1M --iodepth=64 --rw=write \
  --iodepth_batch_submit=64  --iodepth_batch_complete_max=64 | tee -a Diskinfo8.log &&
#### Write LAtency
fio --name=write_latency_test -numjobs=8 \
  --filename=/store/fiotest/testwl --filesize=25G \
  --time_based --ramp_time=2s --runtime=1m \
  --ioengine=libaio --direct=1 --verify=0 --randrepeat=0 \
  --bs=4K --iodepth=4 --rw=randwrite --iodepth_batch_submit=4  \
  --iodepth_batch_complete_max=4 | tee -a Diskinfo8.log && 
#### Read IOPS
fio --name=read_iops_test -numjobs=8 \
  --filename=/store/fiotest/testri --filesize=25G \
  --time_based --ramp_time=2s --runtime=1m \
  --ioengine=libaio --direct=1 --verify=0 --randrepeat=0 \
  --bs=4K --iodepth=256 --rw=randread \
  --iodepth_batch_submit=256  --iodepth_batch_complete_max=256 | tee -a Diskinfo8.log &&

#### Read Throughput
fio --name=read_bandwidth_test -numjobs=8 \
  --filename=/store/fiotest/testrt --filesize=25G \
  --time_based --ramp_time=2s --runtime=1m \
  --ioengine=libaio --direct=1 --verify=0 --randrepeat=0 \
  --bs=1M --iodepth=64 --rw=read --numjobs=16 --offset_increment=100G \
  --iodepth_batch_submit=64  --iodepth_batch_complete_max=64 | tee -a Diskinfo8.log &&
#### Read Latency
fio --name=read_latency_test  -numjobs=8 \
  --filename=/store/fiotest/testrl --filesize=25G \
  --time_based --ramp_time=2s --runtime=1m \
  --ioengine=libaio --direct=1 --verify=0 --randrepeat=0\
  --bs=4K --iodepth=4 --rw=randread \
  --iodepth_batch_submit=4  --iodepth_batch_complete_max=4 | tee -a Diskinfo8.log && echo "end" 

 
 


# prepare for test, after scaled machine
sudo timescaledb-tune --quiet --yes --dry-run | tee -a /path/to/postgresql.conf
service influxdb stop
service clickhouse-server stop
service postgres stop













