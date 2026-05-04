
FROM node:20 AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN rm -rf node_modules && npm ci
RUN npm run build

FROM nginx:alpine

RUN rm -rf /usr/share/nginx/html/*
RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist/AngularPcMatchUp/browser /usr/share/nginx/html

EXPOSE 80