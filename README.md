# 💳 Payments API

## 📖 Visão Geral

A **Payments API** é o microsserviço responsável pelo processamento de pagamentos da plataforma de jogos.

Sua principal função é consumir solicitações de pagamento enviadas pelo **Catalog API**, simular a aprovação da transação e publicar um evento informando o resultado do processamento.

A comunicação entre os microsserviços é realizada de forma **assíncrona** utilizando **RabbitMQ**, garantindo baixo acoplamento, escalabilidade e maior resiliência da arquitetura.

---

# 🚀 Responsabilidades

* Consumir eventos de solicitação de pagamento.
* Simular o processamento de pagamentos.
* Publicar eventos de pagamento processado.
* Registrar logs do processamento.
* Manter a comunicação assíncrona entre os microsserviços.

---

# 🛠️ Tecnologias Utilizadas

* .NET 10
* ASP.NET Core
* RabbitMQ
* Docker
* Kubernetes

---

# 🏗️ Arquitetura

O fluxo de processamento ocorre conforme o diagrama abaixo:

```text
Catalog API
      │
      │ Publica OrderPlacedEvent
      ▼
RabbitMQ (order-placed)
      │
      ▼
Payments API
      │
      │ Processa o pagamento
      │
      │ Publica PaymentProcessedEvent
      ▼
RabbitMQ (payment-processed)
      │
      ▼
Notifications API
```

---

# 🔄 Fluxo de Funcionamento

1. O **Catalog API** publica um evento informando que um pedido foi realizado.
2. O **Payments API** consome o evento da fila `order-placed`.
3. O pagamento é processado (simulação).
4. Um evento `PaymentProcessedEvent` é publicado na fila `payment-processed`.
5. O **Notifications API** consome o evento e realiza o envio da notificação ao usuário.

---

# 📨 Filas Utilizadas

## Consome

| Fila           | Evento             |
| -------------- | ------------------ |
| `order-placed` | `OrderPlacedEvent` |

## Publica

| Fila                | Evento                  |
| ------------------- | ----------------------- |
| `payment-processed` | `PaymentProcessedEvent` |

---

# ⚙️ Configuração

As configurações da aplicação podem ser definidas através de variáveis de ambiente.

## RabbitMQ

```text
RabbitMQ__Host
RabbitMQ__Port
RabbitMQ__User
RabbitMQ__Password
```

---

# ▶️ Executando Localmente

### Restaurar dependências

```bash
dotnet restore
```

### Executar a aplicação

```bash
dotnet run
```

---

# 🐳 Executando com Docker

A **Payments API** pode ser executada de forma independente para fins de desenvolvimento e testes.

### Build da imagem

```bash
docker build -t payments-api .
```

### Executar o container

```bash
docker run -p 5003:8080 payments-api
```

> **Observação:** Ao executar apenas este microsserviço, as funcionalidades que dependem da comunicação com outros serviços (como RabbitMQ e Notifications API) não estarão disponíveis, a menos que essas dependências também estejam em execução.

## 🚀 Executando a solução completa

Para simular o ambiente da aplicação de forma semelhante à produção, o recomendado é utilizar o repositório **Orchestrator**, responsável por orquestrar todos os microsserviços da plataforma.

O repositório do **Orchestrator** possui um **README** com todas as instruções necessárias para configurar e executar a solução completa. Após seguir as etapas descritas nesse repositório, basta executar:

```bash
docker compose up --build
```

Esse comando iniciará todos os componentes necessários da solução, incluindo:

* Users API
* Catalog API
* Payments API
* Notifications API
* RabbitMQ

Dessa forma, será possível testar toda a comunicação entre os microsserviços por meio de eventos, reproduzindo o funcionamento completo da arquitetura.

---

# ☸️ Executando no Kubernetes

### Aplicar os manifests

```bash
kubectl apply -f k8s/
```

### Verificar os recursos

```bash
kubectl get deployments
kubectl get pods
kubectl get services
```

### Consultar os logs

```bash
kubectl logs -f deployment/payments-api
```

---

# 📁 Estrutura do Projeto

```text
Payments.Api
├── Consumers
├── Events
├── Messaging
├── Services
├── Program.cs
├── appsettings.json
└── Dockerfile
```

---

# 🔗 Microsserviços Relacionados

| Microsserviço         | Responsabilidade                                          |
| --------------------- | --------------------------------------------------------- |
| **Users API**         | Gerenciamento de usuários e autenticação.                 |
| **Catalog API**       | Gerenciamento do catálogo de jogos e criação dos pedidos. |
| **Payments API**      | Processamento dos pagamentos.                             |
| **Notifications API** | Envio de notificações após o processamento do pagamento.  |

---

# 🎯 Objetivo

Este microsserviço foi desenvolvido como parte de uma arquitetura baseada em **microsserviços**, utilizando comunicação orientada a eventos com **RabbitMQ** e conteinerização com **Docker** e **Kubernetes**.

O projeto tem como objetivo demonstrar a aplicação de boas práticas de arquitetura, como:

* Comunicação assíncrona entre serviços.
* Baixo acoplamento.
* Escalabilidade.
* Separação de responsabilidades.
* Orquestração de containers com Kubernetes.
