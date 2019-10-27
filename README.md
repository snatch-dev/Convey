# Convey - a simple recipe for .NET Core microservices 
## Read the docs [here](https://convey-stack.github.io) or [see it in action](https://www.youtube.com/watch?v=cxEXx4UT1FI).

# Roadmap

These are the features and integrations that we're planning to work on.

* [x] Consul - custom integration (remove the no longer support Consul package)

* [x] Outbox - implement the outbox pattern in a separate package

* [ ] RabbitMQ - improve the actual implementation using native client, including the support for queue retry, proper handling of connections, and parameters abstraction.

* [ ] Service Mesh - integration with Consul Connect, Envoy and other proxies/sidecars.