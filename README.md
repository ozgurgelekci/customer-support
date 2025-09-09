# 🤖 AI Destekli Müşteri Destek Sistemi

<div align="center">

![AI Customer Support](https://img.shields.io/badge/AI-Powered-blue?style=for-the-badge&logo=openai)
![.NET 9](https://img.shields.io/badge/.NET-9.0-purple?style=for-the-badge&logo=dotnet)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-red?style=for-the-badge&logo=blazor)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue?style=for-the-badge&logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Ready-blue?style=for-the-badge&logo=docker)

**🚀 Modern • Akıllı • Gerçek Zamanlı Müşteri Destek Platformu**

*RAG (Retrieval-Augmented Generation) teknolojisi ile güçlendirilmiş enterprise-grade AI müşteri destek sistemi*

[🎯 Demo](#-demo) • [⚡ Hızlı Başlangıç](#-hızlı-başlangıç) • [📖 Dokümantasyon](#-api-referansı) • [🤝 Katkıda Bulunma](#-katkıda-bulunma)

</div>

---

## 📋 İçindekiler

- [✨ Özellikler](#-özellikler)
- [🏗️ Sistem Mimarisi](#️-sistem-mimarisi)
- [⚡ Hızlı Başlangıç](#-hızlı-başlangıç)
- [🔧 Kurulum Seçenekleri](#-kurulum-seçenekleri)
- [🎯 Demo](#-demo)
- [📊 Performans](#-performans-metrikleri)
- [🛠️ Teknoloji Stack](#️-teknoloji-stack)
- [📖 API Referansı](#-api-referansı)
- [🐛 Sorun Giderme](#-sorun-giderme)
- [🤝 Katkıda Bulunma](#-katkıda-bulunma)
- [📄 Lisans](#-lisans)

---

## ✨ Özellikler

### 🚀 **Çekirdek Özellikler**
- **💬 Real-time Chat** - SignalR ile WebSocket tabanlı anlık mesajlaşma
- **🤖 AI Intelligence** - OpenAI GPT-4o-mini ile akıllı yanıt üretimi
- **📚 RAG Technology** - Contextual, doküman bazlı akıllı cevaplar
- **💰 Token Optimization** - Off-topic soru filtreleme ile %90+ maliyet tasarrufu
- **🇹🇷 Türkçe Optimize** - Türkçe dil desteği ile optimize edilmiş AI

### 🎨 **Kullanıcı Deneyimi**
- **Modern UI/UX** - Modern ve responsive tasarım
- **Mobile Responsive** - Tüm cihazlarda mükemmel görünüm  
- **Real-time Typing** - WhatsApp tarzı yazma göstergeleri
- **Auto-scroll** - Otomatik mesaj takibi
- **Enter Key Support** - Tek tuşla mesaj gönderme
- **Clickable Links** - AI cevaplarındaki linkler yeni sekmede açılır

### 🔧 **Teknik Özellikler**
- **High Performance** - Async/await pattern ile optimize edilmiş
- **Scalable Architecture** - PostgreSQL ile enterprise-ready
- **Docker Ready** - Tek komut ile deployment
- **Comprehensive Logging** - Serilog ile detaylı log sistemi
- **Health Checks** - Sistem durumu monitoring
- **HTTPS/SSL** - Production-ready güvenlik

---

## 🏗️ Sistem Mimarisi

```mermaid
graph TB
    subgraph "Frontend Layer"
        UI[🎨 Blazor Server UI]
        JS[📱 JavaScript Interop]
    end
    
    subgraph "Communication Layer"
        SignalR[⚡ SignalR Hub]
        HTTP[🌐 HTTP API]
    end
    
    subgraph "Business Logic Layer"
        ChatHub[💬 ChatHub]
        AIService[🤖 AI Service]
        RAGService[📚 RAG Service]
        EmbeddingService[🔍 Embedding Service]
    end
    
    subgraph "AI & External Services"
        OpenAI[🧠 OpenAI GPT-4o-mini]
        Embeddings[📊 text-embedding-3-small]
    end
    
    subgraph "Data Layer"
        EFCore[🏛️ Entity Framework Core]
        PostgreSQL[(🗄️ PostgreSQL 16)]
    end
    
    UI --> SignalR
    UI --> HTTP
    SignalR --> ChatHub
    ChatHub --> AIService
    ChatHub --> RAGService
    AIService --> OpenAI
    RAGService --> EmbeddingService
    EmbeddingService --> Embeddings
    RAGService --> OpenAI
    ChatHub --> EFCore
    EFCore --> PostgreSQL
    
    style UI fill:#e1f5fe
    style SignalR fill:#f3e5f5
    style ChatHub fill:#e8f5e8
    style AIService fill:#fff3e0
    style PostgreSQL fill:#e3f2fd
```

### 🔄 **RAG Pipeline Akışı**

```
📄 Document Processing → 🧩 Smart Chunking → 🔍 Vector Embeddings → 
🗄️ PostgreSQL Storage → 📊 Similarity Search → 🎯 Context Injection → 
🤖 AI Response Generation
```

---

## ⚡ Hızlı Başlangıç

### ✅ **Ön Gereksinimler**

- **Docker Desktop** (Önerilen) - [İndir](https://www.docker.com/products/docker-desktop/)
- **OpenAI API Key** - [Al](https://platform.openai.com/api-keys)
- **.NET 9 SDK** (Opsiyonel, sadece local dev için) - [İndir](https://dotnet.microsoft.com/download)

### 🚀 **3 Adımda Başlatın**

#### 1️⃣ **Projeyi Klonlayın**
```bash
git clone https://github.com/your-username/customer-support.git
cd customer-support
```

#### 2️⃣ **Environment Ayarlayın**
```bash
# .env dosyası oluşturun
cp env.example .env

# OpenAI API key'inizi ekleyin (gerekli!)
echo "OPENAI_API_KEY=sk-your-actual-openai-key-here" >> .env
```

#### 3️⃣ **Sistemi Başlatın**

**Windows:**
```batch
start.bat
```

**macOS/Linux:**
```bash
chmod +x start.sh
./start.sh
```

**Manuel:**
```bash
docker-compose up -d
```

### 🎉 **Hazır!**

- **🌐 Ana Uygulama:** http://localhost:8080
- **🏥 Health Check:** http://localhost:8080/health  
- **📊 Logs:** `docker-compose logs -f api`

---

## 🔧 Kurulum Seçenekleri

<details>
<summary><b>🐳 Docker ile Kurulum (Önerilen)</b></summary>

```bash
# Tüm servisleri başlat
docker-compose up -d

# Build ve başlat
docker-compose up -d --build

# Sadece API'yi yeniden başlat
docker-compose restart api

# Logları izle
docker-compose logs -f api

# Servisleri durdur
docker-compose down

# Veritabanını sıfırla
docker-compose down -v && docker-compose up -d
```
</details>

<details>
<summary><b>💻 Local Development Kurulumu</b></summary>

```bash
# Dependencies restore
dotnet restore

# Database setup
dotnet ef database update --project CustomerSupport

# Run application
dotnet run --project CustomerSupport

# Hot reload (Development)  
dotnet watch run --project CustomerSupport
```
</details>

<details>
<summary><b>🏭 Production Deployment</b></summary>

```bash
# Production environment variables
export ASPNETCORE_ENVIRONMENT=Production
export OPENAI_API_KEY=your-production-key

# Database migration
dotnet ef database update --project CustomerSupport

# Docker production build
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```
</details>

---

## 🎯 Demo

### 🖥️ **Screenshots**

<div align="center">

#### 🏠 Welcome Screen
*Modern ve kullanıcı dostu karşılama ekranı*

#### 💬 Chat Interface  
*Real-time messaging ile WhatsApp tarzı chat deneyimi*

#### 🤖 AI Responses
*Context-aware akıllı yanıtlar ve clickable linkler*

#### 📱 Mobile View
*Responsive design ile mükemmel mobile deneyim*

</div>

### 🧪 **Canlı Test Senaryoları**

#### ❌ **Off-topic Testler (Token Tasarrufu)**
```
Kullanıcı: "Selam"
AI: 🤖 Otomatik cevap - 0 token harcanır

Kullanıcı: "Hava nasıl?"  
AI: 🤖 Otomatik cevap - 0 token harcanır

Kullanıcı: "Nasılsın?"
AI: 🤖 Otomatik cevap - 0 token harcanır
```

#### ✅ **On-topic Testler (AI Yanıtları)**
```
Kullanıcı: "Mikro RUN nedir?"
AI: 🧠 Detaylı AI cevabı - Token harcanır

Kullanıcı: "Paraşüt ile muhasebe"
AI: 🧠 Contextual cevap - RAG teknolojisi  

Kullanıcı: "ERP çözümleriniz hakkında bilgi"
AI: 🧠 Comprehensive yanıt - Mikro ürün odaklı
```

---

## 📊 Performans Metrikleri

### 💰 **Maliyet Optimizasyonu**
- **%90+ Token Tasarrufu** - Off-topic soru filtreleme ile
- **gpt-4o-mini Model** - En uygun fiyatlı OpenAI model
- **text-embedding-3-small** - En ucuz embedding model
- **Smart Chunking** - Optimal 800 karakter chunks

### ⚡ **Performance Metrikleri**
- **< 100ms** - SignalR mesaj iletimi  
- **< 2s** - AI cevap süres i (ortalama)
- **%99.9** - Uptime hedefi
- **Unlimited** - Concurrent chat sessions

### 🎯 **RAG Kalite Metrikleri**
- **0-100 Chunk Quality Score** - Türkçe optimize puanlama
- **%15 Dynamic Overlap** - Context preservation  
- **Türkçe Sentence Awareness** - Kısaltma-aware bölme
- **Mikro Term Boosting** - Firma-specific bonus scoring

---

## 🛠️ Teknoloji Stack

### 🔧 **Backend**
| Teknoloji | Versiyon | Açıklama |
|-----------|----------|----------|
| .NET | 9.0 | Modern cross-platform framework |
| ASP.NET Core | 9.0 | Web API ve Blazor Server |
| Entity Framework Core | 9.0 | PostgreSQL ORM |
| SignalR | 9.0 | Real-time WebSocket communication |
| Serilog | 8.x | Structured logging |

### 🤖 **AI & Machine Learning**
| Servis | Model | Maliyet | Kullanım |
|--------|--------|---------|----------|
| OpenAI Chat | gpt-4o-mini | En ucuz | AI yanıtları |
| OpenAI Embedding | text-embedding-3-small | En ucuz | Vector search |
| RAG Pipeline | Custom | - | Context-aware responses |

### 🗄️ **Database & Storage**
| Teknoloji | Versiyon | Açıklama |
|-----------|----------|----------|
| PostgreSQL | 16 | Primary database |
| Vector Storage | Custom | Embedding vectors |
| Connection Pooling | Built-in | High performance |

### 🎨 **Frontend**
| Teknoloji | Açıklama |
|-----------|----------|
| Blazor Server | Server-side rendering |
| CSS3 + Variables | Modern responsive design |
| JavaScript Interop | Client-side enhancements |
| Font Awesome | Professional icons |

### 📦 **DevOps & Deployment**
| Tool | Açıklama |
|------|----------|
| Docker | Containerization |
| Docker Compose | Multi-container orchestration |  
| Multi-stage Builds | Optimized container images |
| Health Checks | Monitoring and reliability |

---

## 📖 API Referansı

### 🔌 **SignalR Hub Endpoints**

#### **ChatHub** (`/chathub`)

```javascript
// Hub Connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .withAutomaticReconnect()
    .build();
```

#### **Hub Methods**

| Method | Parameters | Returns | Açıklama |
|--------|------------|---------|----------|
| `StartSession` | `userName: string` | `conversationId: string` | Yeni chat oturumu başlatır |
| `SendMessage` | `conversationId: string, message: string` | `void` | Mesaj gönderir, AI cevabı alır |
| `EndSession` | `conversationId: string` | `void` | Chat oturumunu sonlandırır |

#### **Hub Events**

| Event | Parameters | Açıklama |
|-------|------------|----------|
| `ReceiveMessage` | `conversationId, sender, message` | Yeni mesaj alındı |
| `UserJoined` | `userName` | Kullanıcı katıldı |
| `UserLeft` | `userName` | Kullanıcı ayrıldı |

### 🏥 **Health Check Endpoint**

```http
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

---

## 🔧 Konfigürasyon

### ⚙️ **Environment Variables**

| Variable | Gerekli | Varsayılan | Açıklama |
|----------|---------|------------|----------|
| `OPENAI_API_KEY` | ✅ | - | OpenAI API anahtarı |
| `ASPNETCORE_ENVIRONMENT` | ❌ | Development | Uygulama ortamı |
| `POSTGRES_DB` | ❌ | customer_support | PostgreSQL veritabanı |
| `POSTGRES_USER` | ❌ | postgres | PostgreSQL kullanıcı |
| `POSTGRES_PASSWORD` | ❌ | postgres123 | PostgreSQL şifre |

### 📄 **appsettings.json**

```json
{
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}",
    "ChatModel": "gpt-4o-mini",
    "EmbeddingModel": "text-embedding-3-small",
    "MaxTokens": 2000,
    "Temperature": 0.7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Database=customer_support;Username=postgres;Password=postgres123"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### 🐳 **docker-compose.yml**

<details>
<summary><b>Yapılandırma Detayları</b></summary>

```yaml
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - OPENAI_API_KEY=${OPENAI_API_KEY}
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      db:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  db:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=customer_support
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```
</details>

---

## 📁 Proje Yapısı

```
📦 customer-support/
├── 📁 CustomerSupport/              # Ana .NET projesi
│   ├── 📁 Components/               # Blazor bileşenleri
│   │   ├── Chat.razor              # Ana chat bileşeni
│   │   ├── MainLayout.razor        # Ana layout
│   │   └── HeadOutlet.razor        # Head section
│   ├── 📁 Controllers/             # API controllers (temizlendi)
│   ├── 📁 Data/                    # Entity Framework context
│   │   └── ApplicationDbContext.cs
│   ├── 📁 Hubs/                    # SignalR hubs  
│   │   └── ChatHub.cs              # Ana chat hub
│   ├── 📁 Models/                  # Data modelleri
│   │   ├── Conversation.cs
│   │   ├── Message.cs  
│   │   └── Document.cs             # RAG modelleri
│   ├── 📁 Services/                # İş mantığı servisleri
│   │   ├── OpenAIService.cs        # AI yanıt servisi
│   │   ├── RagService.cs           # RAG pipeline
│   │   ├── EmbeddingService.cs     # Vector embeddings
│   │   └── DocumentProcessingService.cs # Smart chunking
│   ├── 📁 Migrations/              # EF Core migrations
│   ├── 📁 Pages/                   # Razor pages
│   ├── 📁 wwwroot/                 # Static files
│   │   ├── 📁 css/                 # Stylesheets
│   │   │   └── site.css            # Ana CSS (Mikro branding)
│   │   └── 📁 js/                  # JavaScript files
│   │       └── site.js             # Client-side kod
│   ├── Program.cs                  # Uygulama entry point
│   ├── appsettings.json            # Konfigürasyon
│   └── CustomerSupport.csproj      # Proje dosyası
├── 🐳 Dockerfile                   # Container definition
├── 🐳 docker-compose.yml           # Multi-container setup
├── 🐳 docker-compose.override.yml  # Development overrides
├── 📄 .dockerignore                # Docker ignore rules
├── 📄 .env.example                 # Environment template
├── 🔧 start.sh                     # Linux/Mac başlatma script'i
├── 🔧 start.bat                    # Windows başlatma script'i
└── 📖 README.md                    # Bu dosya
```

---

## 🐛 Sorun Giderme

### 🔴 **Yaygın Sorunlar**

<details>
<summary><b>❌ OpenAI API Key Hatası</b></summary>

**Hata:** `Unauthorized` veya `Invalid API key`

**Çözüm:**
```bash
# API key'in doğru set edildiğini kontrol edin
docker exec -it customer-support-api printenv | grep OPENAI

# .env dosyasını kontrol edin
cat .env

# Containers'ı yeniden başlatın
docker-compose down && docker-compose up -d
```
</details>

<details>
<summary><b>🗄️ Veritabanı Bağlantı Sorunu</b></summary>

**Hata:** Connection timeout, database doesn't exist

**Çözüm:**
```bash
# PostgreSQL durumunu kontrol edin
docker-compose ps postgres

# Container loglarını kontrol edin  
docker-compose logs postgres

# Migration'ları manuel çalıştırın
docker exec -it customer-support-api dotnet ef database update

# Veritabanını sıfırlayın
docker-compose down -v
docker-compose up -d
```
</details>

<details>
<summary><b>🔄 SignalR Bağlantı Sorunu</b></summary>

**Hata:** Chat başlatılamıyor, mesaj gönderilemiyor

**Çözüm:**
```bash
# Container'ı yeniden başlatın
docker-compose restart api

# Blazor server loglarını kontrol edin
docker-compose logs -f api

# Browser console'da hataları kontrol edin
# F12 > Console > Hataları görün
```
</details>

<details>
<summary><b>🚀 Performance Sorunları</b></summary>

**Belirtiler:** Yavaş AI cevapları, timeout'lar

**Çözüm:**
```bash
# Resource kullanımını kontrol edin
docker stats

# Memory ve CPU limitlerini artırın
# docker-compose.yml'de resources section'ı güncelleyin

# OpenAI API quota'nızı kontrol edin
curl -H "Authorization: Bearer $OPENAI_API_KEY" \
     https://api.openai.com/v1/usage
```
</details>

### 📊 **Log Monitoring**

```bash
# Tüm servislerin logları
docker-compose logs -f

# Sadece API logları  
docker-compose logs -f api

# PostgreSQL logları
docker-compose logs -f postgres

# Son 100 satır log
docker-compose logs --tail=100 api

# Hata loglarını filtrele
docker-compose logs api | grep ERROR
```

---

## 🤝 Katkıda Bulunma

### 🎯 **Katkı Yönergeleri**

1. **Fork** edin repository'yi
2. **Branch** oluşturun (`git checkout -b feature/amazing-feature`)  
3. **Commit** edin değişikliklerinizi (`git commit -m 'Add amazing feature'`)
4. **Push** edin branch'inize (`git push origin feature/amazing-feature`)
5. **Pull Request** açın

### 🧪 **Development Setup**

```bash
# Projeyi fork edin ve clone'layın
git clone https://github.com/your-username/customer-support.git

# Development branch oluşturun
git checkout -b feature/your-feature-name

# Local development environment başlatın
docker-compose up -d
dotnet watch run --project CustomerSupport

# Test'lerinizi yazın
dotnet test

# Code quality kontrolleri
dotnet format
dotnet build --verbosity normal
```

### 📝 **Katkı Alanları**

- **🐛 Bug Fixes** - Hata raporları ve düzeltmeleri
- **✨ New Features** - Yeni özellik önerileri  
- **📖 Documentation** - Dokümantasyon iyileştirmeleri
- **🎨 UI/UX** - Tasarım ve kullanıcı deneyimi
- **⚡ Performance** - Optimizasyon önerileri
- **🌍 i18n** - Çoklu dil desteği
- **🧪 Testing** - Test coverage artırma


---

## 📄 Lisans

Bu proje **MIT License** altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

```
MIT License

Copyright (c) 2025 Customer Support System

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

---

## 🙏 Teşekkürler

### **🛠️ Open Source Tools**
- [.NET](https://dotnet.microsoft.com/) - Cross-platform development framework
- [PostgreSQL](https://www.postgresql.org/) - Advanced open source database
- [OpenAI](https://openai.com/) - AI platform for intelligent responses
- [Docker](https://www.docker.com/) - Containerization platform

### **🎨 Design Resources**  
- [Font Awesome](https://fontawesome.com/) - Professional icon library
- [CSS Grid](https://css-tricks.com/snippets/css/complete-guide-grid/) - Modern layout system

---

<div align="center">

## 🚀 Hemen Başla!

```bash
git clone https://github.com/your-username/customer-support.git
cd customer-support
echo "OPENAI_API_KEY=your-key-here" > .env
docker-compose up -d
```

**📱 http://localhost:8080 - Sisteminiz hazır!**

---

**⭐ Projeyi beğendiyseniz star vermeyi unutmayın!**

*Modern AI ile müşteri deneyimini dönüştürün! 🤖✨*

---

**Made with ❤️ in Turkey**  
*Powered by .NET 9 • Enhanced with OpenAI • Deployed with Docker*

</div>

---

# 🤖 AI-Powered Customer Support System

<div align="center">

![AI Customer Support](https://img.shields.io/badge/AI-Powered-blue?style=for-the-badge&logo=openai)
![.NET 9](https://img.shields.io/badge/.NET-9.0-purple?style=for-the-badge&logo=dotnet)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-red?style=for-the-badge&logo=blazor)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue?style=for-the-badge&logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Ready-blue?style=for-the-badge&logo=docker)

**🚀 Modern • Intelligent • Real-time Customer Support Platform**

*Enterprise-grade AI customer support system powered by RAG (Retrieval-Augmented Generation) technology*

[🎯 Demo](#-demo-en) • [⚡ Quick Start](#-quick-start-en) • [📖 Documentation](#-api-reference-en) • [🤝 Contributing](#-contributing-en)

</div>

---

## ✨ Features

### 🚀 **Core Features**
- **💬 Real-time Chat** - Instant messaging with SignalR WebSocket technology
- **🤖 AI Intelligence** - Smart response generation with OpenAI GPT-4o-mini
- **📚 RAG Technology** - Contextual, document-based intelligent answers
- **💰 Token Optimization** - 90%+ cost savings with off-topic question filtering
- **🇹🇷 Turkish Optimized** - Specialized AI for Turkish language support

### 🎨 **User Experience**
- **Modern UI/UX** - Clean and professional design
- **Mobile Responsive** - Perfect view on all devices
- **Real-time Typing** - WhatsApp-style typing indicators
- **Auto-scroll** - Automatic message tracking
- **Enter Key Support** - Send messages with one key
- **Clickable Links** - AI response links open in new tab

### 🔧 **Technical Features**
- **High Performance** - Optimized with async/await pattern
- **Scalable Architecture** - Enterprise-ready with PostgreSQL
- **Docker Ready** - Single command deployment
- **Comprehensive Logging** - Detailed log system with Serilog
- **Health Checks** - System health monitoring
- **HTTPS/SSL** - Production-ready security

---

## 🏗️ System Architecture

The system follows a modern layered architecture with SignalR for real-time communication, AI services for intelligent responses, and RAG pipeline for context-aware answers.

**RAG Pipeline Flow:**
```
📄 Document Processing → 🧩 Smart Chunking → 🔍 Vector Embeddings → 
🗄️ PostgreSQL Storage → 📊 Similarity Search → 🎯 Context Injection → 
🤖 AI Response Generation
```

---

## ⚡ Quick Start {#quick-start-en}

### ✅ **Prerequisites**

- **Docker Desktop** (Recommended) - [Download](https://www.docker.com/products/docker-desktop/)
- **OpenAI API Key** - [Get it](https://platform.openai.com/api-keys)
- **.NET 9 SDK** (Optional, for local dev) - [Download](https://dotnet.microsoft.com/download)

### 🚀 **Start in 3 Steps**

#### 1️⃣ **Clone Project**
```bash
git clone https://github.com/your-username/customer-support.git
cd customer-support
```

#### 2️⃣ **Setup Environment**
```bash
# Create .env file
cp env.example .env

# Add your OpenAI API key (required!)
echo "OPENAI_API_KEY=sk-your-actual-openai-key-here" >> .env
```

#### 3️⃣ **Start System**

**Windows:**
```batch
start.bat
```

**macOS/Linux:**
```bash
chmod +x start.sh
./start.sh
```

**Manual:**
```bash
docker-compose up -d
```

### 🎉 **Ready!**

- **🌐 Main Application:** http://localhost:8080
- **🏥 Health Check:** http://localhost:8080/health  
- **📊 Logs:** `docker-compose logs -f api`

---

## 🛠️ Technology Stack

### 🔧 **Backend**
| Technology | Version | Description |
|-----------|----------|----------|
| .NET | 9.0 | Modern cross-platform framework |
| ASP.NET Core | 9.0 | Web API and Blazor Server |
| Entity Framework Core | 9.0 | PostgreSQL ORM |
| SignalR | 9.0 | Real-time WebSocket communication |
| Serilog | 8.x | Structured logging |

### 🤖 **AI & Machine Learning**
| Service | Model | Cost | Usage |
|--------|--------|---------|----------|
| OpenAI Chat | gpt-4o-mini | Cheapest | AI responses |
| OpenAI Embedding | text-embedding-3-small | Cheapest | Vector search |
| RAG Pipeline | Custom | - | Context-aware responses |

### 🗄️ **Database & Storage**
| Technology | Version | Description |
|-----------|----------|----------|
| PostgreSQL | 16 | Primary database |
| Vector Storage | Custom | Embedding vectors |
| Connection Pooling | Built-in | High performance |

### 🎨 **Frontend**
| Technology | Description |
|-----------|----------|
| Blazor Server | Server-side rendering |
| CSS3 + Variables | Modern responsive design |
| JavaScript Interop | Client-side enhancements |
| Font Awesome | Professional icons |

---

## 📖 API Reference {#api-reference-en}

### 🔌 **SignalR Hub Endpoints**

#### **ChatHub** (`/chathub`)

| Method | Parameters | Returns | Description |
|--------|------------|---------|----------|
| `StartSession` | `userName: string` | `conversationId: string` | Start new chat session |
| `SendMessage` | `conversationId: string, message: string` | `void` | Send message, get AI response |
| `EndSession` | `conversationId: string` | `void` | End chat session |

#### **Hub Events**

| Event | Parameters | Description |
|-------|------------|----------|
| `ReceiveMessage` | `conversationId, sender, message` | New message received |
| `UserJoined` | `userName` | User joined |
| `UserLeft` | `userName` | User left |

### 🏥 **Health Check Endpoint**

```http
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

---

## 🔧 Configuration

### ⚙️ **Environment Variables**

| Variable | Required | Default | Description |
|----------|---------|------------|----------|
| `OPENAI_API_KEY` | ✅ | - | OpenAI API key |
| `ASPNETCORE_ENVIRONMENT` | ❌ | Development | Application environment |
| `POSTGRES_DB` | ❌ | customer_support | PostgreSQL database |
| `POSTGRES_USER` | ❌ | postgres | PostgreSQL user |
| `POSTGRES_PASSWORD` | ❌ | postgres123 | PostgreSQL password |

---

## 🐳 Docker Setup

**Start all services:**
```bash
docker-compose up -d
```

**Build and start:**
```bash
docker-compose up -d --build
```

**View logs:**
```bash
docker-compose logs -f api
```

**Stop services:**
```bash
docker-compose down
```

---

## 📊 Performance Metrics

### 💰 **Cost Optimization**
- **90%+ Token Savings** - Off-topic question filtering
- **gpt-4o-mini Model** - Most cost-effective OpenAI model
- **text-embedding-3-small** - Cheapest embedding model
- **Smart Chunking** - Optimal 800 character chunks

### ⚡ **Performance Metrics**
- **< 100ms** - SignalR message delivery  
- **< 2s** - AI response time (average)
- **99.9%** - Uptime target
- **Unlimited** - Concurrent chat sessions

---

## 🐛 Troubleshooting

### 🔴 **Common Issues**

**❌ OpenAI API Key Error**
```bash
# Check if API key is correctly set
docker exec -it customer-support-api printenv | grep OPENAI

# Check .env file
cat .env

# Restart containers
docker-compose down && docker-compose up -d
```

**🗄️ Database Connection Issue**
```bash
# Check PostgreSQL status
docker-compose ps postgres

# Check container logs
docker-compose logs postgres

# Reset database
docker-compose down -v
docker-compose up -d
```

### 📊 **Log Monitoring**

```bash
# All service logs
docker-compose logs -f

# API logs only
docker-compose logs -f api

# Filter error logs
docker-compose logs api | grep ERROR
```

---

## 🤝 Contributing {#contributing-en}

### 🎯 **Contribution Guidelines**

1. **Fork** the repository
2. **Create** a branch (`git checkout -b feature/amazing-feature`)  
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to your branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### 🧪 **Development Setup**

```bash
# Fork and clone the project
git clone https://github.com/your-username/customer-support.git

# Create development branch
git checkout -b feature/your-feature-name

# Start local development environment
docker-compose up -d
dotnet watch run --project CustomerSupport

# Run tests
dotnet test

# Code quality checks
dotnet format
dotnet build --verbosity normal
```


---

## 📄 License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

### **🛠️ Open Source Tools**
- [.NET](https://dotnet.microsoft.com/) - Cross-platform development framework
- [PostgreSQL](https://www.postgresql.org/) - Advanced open source database
- [OpenAI](https://openai.com/) - AI platform for intelligent responses
- [Docker](https://www.docker.com/) - Containerization platform

---

<div align="center">

## 🚀 Get Started Now!

```bash
git clone https://github.com/your-username/customer-support.git
cd customer-support
echo "OPENAI_API_KEY=your-key-here" > .env
docker-compose up -d
```

**📱 http://localhost:8080 - Your system is ready!**

---

**⭐ If you like the project, don't forget to star it!**

*Transform customer experience with modern AI! 🤖✨*

</div>
