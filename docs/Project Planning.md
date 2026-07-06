===============================================================================
COPILOT PLANNING & INSTRUCTIONS FILE: PROJECT OMNIRAG
===============================================================================
File Target: .github/copilot-instructions.md OR copilot-instructions.md
Created: June 2026
===============================================================================

[1. PROJECT OVERVIEW & ARCHITECTURE]
-------------------------------------------------------------------------------
OmniRAG is an enterprise-grade Retrieval-Augmented Generation (RAG) platform. 
It allows users to upload documents and query both document contents and 
structured database data using Azure OpenAI (GPT-4).

High-Level Architecture Flow:
  [ Angular 21 UI ] ---> [ .NET Core Web API ]
                                 |
        +------------------------+------------------------+
        |                        |                        |
  [ Azure KeyVault ]       [ Azure Blob Storage ]   [ Azure OpenAI (GPT-4) ]
                                 |
                       [ Azure SQL Database ]


[2. TECHNOLOGY STACK & STRICT RULES]
-------------------------------------------------------------------------------
A. Backend: .NET Core 9.0+ / 8.0
   * Patterns: Domain-Driven Design (DDD), SOLID, Clean Architecture, CQRS.
   * Rules:
     - OmniRAG.Domain: Completely isolated. No dependencies on EF Core, Azure 
       SDKs, or external frameworks. Contains Entities, Aggregates, Value 
       Objects, and Domain Events.
     - OmniRAG.Application: Use cases (MediatR Handlers), Mappers, and DTOs.
     - OmniRAG.Infrastructure: SQL (EF Core), Blob Storage, Azure OpenAI, 
       and KeyVault integration.
     - OmniRAG.Api: Controllers or Minimal APIs responsible only for routing, 
       authentication, and HTTP response handling.

B. Frontend: Angular v21 (Latest Stable)
   * Style: Standalone Components (No NgModules), Signal-based state, strict TS.
   * Rules: No legacy RxJS component state piping where Angular Signals can 
     be used (signal, computed, effect). Use toSignal for HTTP requests.


[3. DIRECTORY & PROJECT STRUCTURE]
-------------------------------------------------------------------------------
OmniRAG/
│
├── backend/
│   ├── OmniRAG.sln
│   ├── src/
│   │   ├── OmniRAG.Domain/          # Entities, Aggregates, Value Objects
│   │   ├── OmniRAG.Application/     # CQRS Commands/Queries, DTOs
│   │   ├── OmniRAG.Infrastructure/  # SQL, Blob, Azure OpenAI, KeyVault
│   │   └── OmniRAG.Api/             # Controllers, Minimal APIs, Program.cs
│   └── tests/
│
└── frontend/
    └── omnirag-ui/                  # Angular Workspace
        ├── src/
        │   ├── app/
        │   │   ├── core/            # Auth, Interceptors, Guard, API Services
        │   │   ├── shared/          # Buttons, Spinners, Dialogs (Generic)
        │   │   └── features/        # Feature modules
        │   │       ├── dashboard/
        │   │       └── chatbot/     # UI Chat, uploads, source citations


[4. FEATURE IMPLEMENTATION MILESTONES]
-------------------------------------------------------------------------------
[ ] MILESTONE 1: Core Setup & Security (Infrastructure Base)
    * Configure Azure KeyVault integration in OmniRAG.Api 'Program.cs' to 
      fetch connection strings and AI keys securely.
    * Set up DbContext in OmniRAG.Infrastructure targeting Azure SQL using 
      managed identities.
    * Initialize Angular UI with standard layout (Sidebar, Header, Main).

[ ] MILESTONE 2: File Ingestion Pipeline (RAG Setup)
    * Domain: Create a 'Document' Aggregate Root tracking FileId, FileName, 
      BlobUrl, Status (Processing, Indexed, Failed), and UploadDate.
    * Infrastructure (Blob): Implement IBlobStorageService to upload raw 
      documents to Azure Blob Storage.
    * Infrastructure (AI/Vectorization): Implement a background handler that 
      chunks documents and generates embeddings using Azure OpenAI 
      'text-embedding-3-large'. Store indices in Azure SQL (vector support).

[ ] MILESTONE 3: The Chatbot Engine (Hybrid Querying)
    * Application: Create a GetChatResponseQuery handled via MediatR.
    * AI Logic (GPT-4):
      - Step A: Take user natural language prompt.
      - Step B: Perform semantic search against document chunks + structural 
        extraction queries against Azure SQL via dynamic EF queries or views.
      - Step C: Inject both contexts into the GPT-4 system prompt.
    * API: Expose a streaming endpoint (IAsyncEnumerable<string>) for real-time 
      typewriter-effect chat responses.

[ ] MILESTONE 4: Frontend UI Integration
    * Build a clean chatbot interface (features/chatbot) featuring modern 
      chat bubbles, markdown rendering for code blocks, and source citations.
    * Add a drag-and-drop file upload component with active progress tracking.
    * Bind API streams to Angular Signals for real-time UI updates.


[5. CODE STYLE & COPILOT GENERATION DIRECTIVES]
-------------------------------------------------------------------------------
* C# / .NET Guidance:
  "Use strictly typed entities with strongly-typed IDs where possible to 
   enforce DDD principles. Avoid anemic domain models; ensure entities 
   validate their own state via internal business rules or factory methods. 
   Implement global exception handling using IExceptionHandler (.NET style)."

* Angular Guidance:
  "Write components using templateUrl and styleUrl separate files if they 
   exceed 50 lines. Utilize the new control flow syntax (@if, @for, @switch). 
   Ensure all components are marked standalone: true. Prefer Signals over 
   standard BehaviorSubject for local component state."


[6. ENVIRONMENT & CONFIGURATION CHECKLISTS]
-------------------------------------------------------------------------------
A. Backend appsettings.json Template:
{
  "Azure": {
    "KeyVaultUri": "https://kv-omnirag.vault.azure.net/",
    "Storage": {
      "ContainerName": "documents"
    },
    "OpenAI": {
      "Endpoint": "https://ai-omnirag.openai.azure.com/",
      "ChatDeploymentName": "gpt-4",
      "EmbeddingDeploymentName": "text-embedding"
    }
  }
}

B. Frontend Environment Template (environment.ts):
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api'
};
===============================================================================