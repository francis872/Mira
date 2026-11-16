#!/bin/sh
# En una base nueva Docker ejecuta este archivo una sola vez; delega en las migraciones controladas.
sh /migrations/apply.sh
