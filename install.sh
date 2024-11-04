#!/bin/bash

clear
base64 -d <<< "CiAvJCQgICAgICAvJCQgICAgICAgICAgICAgICAgICAgICAgICAgICAvJCQgLyQkICAgICAgICAgIAp8ICQkICAvJCB8ICQkICAgICAgICAgICAgICAgICAgICAgICAgICB8ICQkfCAkJCAgICAgICAgICAKfCAkJCAvJCQkfCAkJCAgLyQkJCQkJCAgIC8kJCQkJCQgICAvJCQkJCQkJHwgJCQgIC8kJCQkJCQgCnwgJCQvJCQgJCQgJCQgLyQkX18gICQkIC8kJF9fICAkJCAvJCRfXyAgJCR8ICQkIC8kJF9fICAkJAp8ICQkJCRfICAkJCQkfCAkJCAgXCAkJHwgJCQgIFxfXy98ICQkICB8ICQkfCAkJHwgJCQkJCQkJCQKfCAkJCQvIFwgICQkJHwgJCQgIHwgJCR8ICQkICAgICAgfCAkJCAgfCAkJHwgJCR8ICQkX19fX18vCnwgJCQvICAgXCAgJCR8ICAkJCQkJCQvfCAkJCAgICAgIHwgICQkJCQkJCR8ICQkfCAgJCQkJCQkJAp8X18vICAgICBcX18vIFxfX19fX18vIHxfXy8gICAgICAgXF9fX19fX18vfF9fLyBcX19fX19fXy8KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAK"

echo "Welcome to the Wordle server installation script!"
echo "This script will guide you through the installation process."
echo "Please make sure you have Docker and Docker Compose installed. If you want leave default values, just press Enter."
echo ""

echo "Database configuration"
echo "----------------------"
echo ""

# Prompt for environment variables
read -p "Enter database user (default: kaktus): " DATABASE_USER
DATABASE_USER=${DATABASE_USER:-kaktus} # Default value if none provided
read -sp "Enter database password: (default: jkkkakalf84ZZFaIkcula#_??) " DATABASE_PASSWORD
DATABASE_PASSWORD=${DATABASE_PASSWORD:-jkkkakalf84ZZFaIkcula#_??} # Default value if none provided
echo ""
read -p "Enter database port (default: 3306): " DATABASE_PORT
DATABASE_PORT=${DATABASE_PORT:-3306} # Default value if none provided

clear
base64 -d <<< "ICAgIF9fXyAgICAgICAgICAgICAgICBfXyAgICAgICAgICAgX18gICAgICAgICAgICAgX19fICAgX19fXyBfX18gIF9fIF9fCiAgIC8gICB8ICBfX19fICBfX19fIF8vIC9fICBfX19fX19fLyAvXyAgX19fXyBfICAgfF9fIFwgLyBfXyBcX18gXC8gLy8gLwogIC8gL3wgfCAvIF9fIFwvIF9fIGAvIC8gLyAvIC8gX19fLyBfXyBcLyBfXyBgLyAgIF9fLyAvLyAvIC8gL18vIC8gLy8gL18KIC8gX19fIHwvIC9fLyAvIC9fLyAvIC8gL18vIC8gL19fLyAvIC8gLyAvXy8gLyAgIC8gX18vLyAvXy8gLyBfXy9fXyAgX18vCi9fLyAgfF8vIC5fX18vXF9fLF8vXy9cX18sXy9cX19fL18vIC9fL1xfXyxfLyAgIC9fX19fL1xfX19fL19fX18vIC9fLyAgIAogICAgICAvXy8gCgo="

echo "Server configuration"
echo "----------------------"
echo ""

read -p "Enter server port (default: 5000): " SERVER_PORT
SERVER_PORT=${SERVER_PORT:-5000} # Default value if none provided
read -sp "Enter JWT secret: " JWT_SECRET
echo ""
read -p "Enter JWT issuer (default: https://worlde.kaktusgame.eu): " JWT_ISSUER
JWT_ISSUER=${JWT_ISSUER:-https://wordle.kaktusgame.eu} # Default value if none provided

clear

base64 -d <<< "ICAgIF9fXyAgICAgICAgICAgICAgICBfXyAgICAgICAgICAgX18gICAgICAgICAgICAgX19fICAgX19fXyBfX18gIF9fIF9fCiAgIC8gICB8ICBfX19fICBfX19fIF8vIC9fICBfX19fX19fLyAvXyAgX19fXyBfICAgfF9fIFwgLyBfXyBcX18gXC8gLy8gLwogIC8gL3wgfCAvIF9fIFwvIF9fIGAvIC8gLyAvIC8gX19fLyBfXyBcLyBfXyBgLyAgIF9fLyAvLyAvIC8gL18vIC8gLy8gL18KIC8gX19fIHwvIC9fLyAvIC9fLyAvIC8gL18vIC8gL19fLyAvIC8gLyAvXy8gLyAgIC8gX18vLyAvXy8gLyBfXy9fXyAgX18vCi9fLyAgfF8vIC5fX18vXF9fLF8vXy9cX18sXy9cX19fL18vIC9fL1xfXyxfLyAgIC9fX19fL1xfX19fL19fX18vIC9fLyAgIAogICAgICAvXy8gCgo="
echo "Pulling images from Docker Hub..."



cat << EOF > ./Docker/.env
DATABASE_USER=$DATABASE_USER
DATABASE_PASSWORD=$DATABASE_PASSWORD
JWT_ISSUER=$JWT_ISSUER
DOCKER_WEB_PORT=$SERVER_PORT
DATABASE_PORT=$DATABASE_PORT
EOF

# Append JWT_SECRET to .env file ONLY if it is provided
if [ -n "$JWT_SECRET" ]; then
    echo "JWT_SECRET=$JWT_SECRET" >> ./Docker/.env
fi

cat << EOF > ./Server/.env
JWT_ISSUER=$JWT_ISSUER
EOF

if [ -n "$JWT_SECRET" ]; then
    echo "JWT_SECRET=$JWT_SECRET" >> ./Server/.env
fi

cd Docker
docker compose build
docker compose up -d

echo "Installation complete!"