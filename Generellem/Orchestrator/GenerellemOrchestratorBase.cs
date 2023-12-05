﻿using Generellem.DataSource;
using Generellem.Llm;
using Generellem.Rag;

namespace Generellem.Orchestrator;

/// <summary>
/// Coordinates <see cref="IDocumentSource"/>, <see cref="ILlm"/>, and <see cref="IRag"/>
/// to coordinate document processing and querying.
/// </summary>
public abstract class GenerellemOrchestratorBase
{
    protected virtual IDocumentSource DocSource { get; init; }
    protected virtual ILlm Llm { get; init; }
    protected virtual IRag Rag { get; init; }

    public GenerellemOrchestratorBase(IDocumentSource docSource, ILlm llm, IRag rag)
    {
        this.DocSource = docSource;
        this.Llm = llm;
        this.Rag = rag;
    }

    public abstract Task ProcessFilesAsync(CancellationToken cancellationToken);

    public abstract Task<string> AskAsync(string message, CancellationToken  cancellationToken);
}