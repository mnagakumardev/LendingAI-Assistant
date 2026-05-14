# LendingAI Assistant 🏦

An intelligent AI-powered loan application platform built with **.NET 8 + React + Azure + OpenAI**.

## 🚀 Features

- **Smart Loan Application Form** - Intuitive UI for collecting applicant information
- **AI-Powered Assessment** - Azure OpenAI integration for intelligent loan analysis
- **Document Processing** - Upload and process supporting documents
- **Credit Scoring** - Automated credit score calculation
- **Real-time Dashboard** - Monitor application status in real-time
- **Secure Data Storage** - Azure Cosmos DB integration
- **Scalable Architecture** - Microservices-ready design

## 🏗️ Architecture

```
LendingAI Assistant
├── Backend (.NET 8)
│   ├── API Layer (ASP.NET Core)
│   ├── Business Logic (Services)
│   ├── Data Access (Cosmos DB)
│   └── AI Integration (Azure OpenAI)
├── Frontend (React + TypeScript)
│   ├── Application Form
│   ├── Dashboard
│   └── Assessment View
└── Infrastructure
    ├── Azure Cosmos DB
    ├── Azure Storage
    ├── Azure OpenAI
    └── Application Insights
```

## 📋 Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- Azure Subscription with:
  - Cosmos DB instance
  - Azure Storage Account
  - Azure OpenAI service
  - Application Insights

## 🔧 Configuration

### Backend Setup

1. **Create appsettings.json** in `backend/LendingAI.API/`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<your-resource>.openai.azure.com/",
    "ApiKey": "your-api-key"
  },
  "CosmosDB": {
    "ConnectionString": "your-cosmos-connection-string",
    "DatabaseName": "LendingAI"
  },
  "AzureStorage": {
    "ConnectionString": "your-storage-connection-string"
  }
}
```

2. **Install Dependencies**:

```bash
cd backend
dotnet restore
dotnet build
```

### Frontend Setup

1. **Install Dependencies**:

```bash
cd frontend
npm install
```

2. **Create .env.local**:

```
REACT_APP_API_URL=http://localhost:5000/api
```

3. **Start Development Server**:

```bash
npm run dev
```

## 🚀 Running with Docker Compose

```bash
docker-compose up --build
```

This will start:
- Backend API on `http://localhost:5000`
- Frontend on `http://localhost:3000`
- Cosmos DB Emulator on `http://localhost:8081`

## 📡 API Endpoints

### Loan Applications

- `POST /api/loanapplication/create` - Create new application
- `GET /api/loanapplication/{id}` - Get application details
- `POST /api/loanapplication/{id}/ai-assessment` - Run AI assessment
- `POST /api/loanapplication/{id}/documents` - Upload documents
- `POST /api/loanapplication/{id}/credit-score` - Calculate credit score
- `POST /api/loanapplication/{id}/submit` - Submit application

## 🤖 AI Assessment Features

The AI agent analyzes:
- **Debt-to-Income Ratio** - Financial capacity assessment
- **Loan-to-Income Ratio** - Loan sustainability
- **Employment Status** - Income stability
- **Age Factor** - Repayment capability
- **Document Analysis** - Document authenticity

## 📊 Workflow

1. **Create Application** - User fills out the form
2. **Upload Documents** - Submit supporting documents
3. **AI Assessment** - OpenAI analyzes the application
4. **Credit Scoring** - Automated score calculation
5. **Submission** - Final application submission
6. **Decision** - System provides approval/denial recommendation

## 🧪 Testing

```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend
npm test
```

## 📦 Deployment

### Azure App Service

```bash
# Publish backend
dotnet publish -c Release

# Deploy to App Service
az webapp up --name <app-name> --resource-group <rg-name>
```

### Azure Container Registry

```bash
# Build and push Docker images
docker build -f Dockerfile.backend -t lending-ai-api .
docker tag lending-ai-api <registry>.azurecr.io/lending-ai-api:latest
docker push <registry>.azurecr.io/lending-ai-api:latest
```

## 🔐 Security Considerations

- ✅ Use Azure Managed Identity for service authentication
- ✅ Enable HTTPS/TLS encryption
- ✅ Implement role-based access control (RBAC)
- ✅ Encrypt sensitive data at rest and in transit
- ✅ Use Azure Key Vault for secrets management

## 📝 Environment Variables

```bash
ASPCORE_ENVIRONMENT=Production
AZURE_OPENAI_ENDPOINT=your-endpoint
AZURE_OPENAI_API_KEY=your-key
COSMOS_CONNECTION_STRING=your-connection-string
AZURE_STORAGE_CONNECTION_STRING=your-connection-string
ApplicationInsights__InstrumentationKey=your-key
```

## 🛠️ Troubleshooting

### API Connection Issues
- Verify CORS configuration in backend
- Check firewall rules
- Ensure services are running

### Cosmos DB Issues
- Verify connection string
- Check database/container existence
- Monitor quota usage

### OpenAI Integration
- Verify API key and endpoint
- Check model deployment name
- Monitor token usage

## 📚 Resources

- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Cosmos DB .NET SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/sql/sql-api-dotnet-v3)
- [React Router](https://reactrouter.com/)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)

## 📄 License

MIT License - See LICENSE file for details

## 👥 Contributing

Contributions welcome! Please read CONTRIBUTING.md for details.

## 📧 Support

For issues and questions, open a GitHub issue or contact support@lendingai.com

---

**Built with ❤️ using .NET, React, and Azure AI**