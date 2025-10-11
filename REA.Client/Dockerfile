FROM node:20-alpine

WORKDIR /app

# Copy package files
COPY package*.json ./

# Clean install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN npm run build

EXPOSE 3000

ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

CMD ["npm", "run", "preview"]