# slashbot


### Docker support

```docker

services:
  slashbot:
    image: ${DOCKER_REGISTRY-}slashbot
    build:
      context: .
      dockerfile: ./Dockerfile
    environment: 
      - Discord:Token=<token>
      - Discord:DefaultActivity=Commands...
      = Discord:DefaultActivityType=ListeningTo
```