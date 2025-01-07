FROM node:18 as build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install --force

COPY . .
RUN npm run build

FROM node:18 AS serve
WORKDIR /app
COPY --from=build /app/dist ./build

RUN npm install -g serve
CMD ["serve", "-s", "build"]