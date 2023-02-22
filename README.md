# slashbot


### Docker support

```docker
version: '3.4'

services:
  slashbot:
    image: ${DOCKER_REGISTRY-}slashbot
    build:
      context: .
      dockerfile: ./Dockerfile
    environment: 
      - Discord_Token=<token>
      - Discord_DefaultActivity=Commands...
      = Discord_DefaultActivityType=ListeningTo
```