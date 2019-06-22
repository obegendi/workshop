[![Build status](https://ci.appveyor.com/api/projects/status/vvx48pjw4aahw9fe/branch/master?svg=true)](https://ci.appveyor.com/project/obegendi/workshop/branch/master)


# Project Description
For start project
```
docker-compose up --build -d
```

By default, the dev box exposes the following ports:
```
9200: Elasticsearch HTTP
9300: Elasticsearch TCP transport
5601: Kibana
3000: MongoClient
27017: MongoDb
15672: RabbitMQ Admin Panel
5672: RabbitMQ AMPQ
6379: Redis
```
By default API ports
```
5010: product.api
5011: read.api
5012: search.api
5013: write.api
```
All api support Swagger by default.


Notable APIs

```
POST 	/api/products/ 	
GET 	/api/products/123/ 	
GET 	/api/products/?query=abc
```

