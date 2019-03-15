# 4210161013_udp_encapsulation
School homework to make an udp encapsulation program for sending and receiving data

Steps
1. The server side would start listening for clients
2. In the client side we type a structured message so that it can be converted into struct
3. The struct containing the data (coords, rotation, HP, etc) will be encapsulated by marshal, converting them to bytes
4. The marshal bytes of converted struct will be sent to the server using UDP
5. At the server, the marshal bytes well be de-encapsulated to their original struct structure
6. The struct structure values will be printed out
