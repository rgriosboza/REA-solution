FROM postgres:15-alpine

# Copy initialization scripts
COPY scripts/ /docker-entrypoint-initdb.d/

# Set environment variables
ENV POSTGRES_DB=REA_DB
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=password

EXPOSE 5432