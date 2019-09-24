# SwaggyNetty

## System definition
### Minimums krav
- 2 players
- Grid map
- 10 sekunders tick
- Pathfinding
- Én enhed per celle

### Ideelt
- Minimum 20 players
- Home instance (1 node pr. instance)
- Loot camps & battle system
- PvP
- Zone opdelt world map

## To do

### Base TCP/IP - netværks stack
- ❌ Disconnect detection / pinging
  - ✔ Remove player server-side
  - ❌ Remove player client-side
- ✔ First time connection message (server => client)
- ✔ Update player positions on clients. Probably new actor class

## Systemintegration
- ❌ Lav kald mellem client - server asynkrone
- ❌ Måske skift fra JSON til XML, vurder fordele / ulemper
- ❌ Kommunikations node mellem client og server
- ...

