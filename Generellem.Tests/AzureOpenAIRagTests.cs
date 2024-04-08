﻿using Azure;
using Azure.AI.OpenAI;

using Generellem.Document.DocumentTypes;
using Generellem.Embedding;
using Generellem.Llm;
using Generellem.Rag.AzureOpenAI;
using Generellem.Repository;
using Generellem.Services;
using Generellem.Services.Azure;
using Generellem.Tests;

using Microsoft.Extensions.Logging;

namespace Generellem.Rag.Tests;

public class AzureOpenAIRagTests
{
    readonly Mock<IAzureSearchService> azSearchSvcMock = new();
    readonly Mock<IDocumentHashRepository> docHashRepMock = new();
    readonly Mock<IDocumentType> docTypeMock = new();
    readonly Mock<IDynamicConfiguration> configMock = new();
    readonly Mock<IEmbedding> embedMock = new();
    readonly Mock<ILogger<AzureOpenAIRag>> logMock = new();

    readonly Mock<OpenAIClient> openAIClientMock = new();
    readonly Mock<Response<Embeddings>> embeddingsMock = new();
    readonly Mock<LlmClientFactory> llmClientFactMock;

    readonly AzureOpenAIRag azureOpenAIRag;
    readonly ReadOnlyMemory<float> embedding;

    public AzureOpenAIRagTests()
    {
        docTypeMock
            .Setup(doc => doc.GetTextAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("text content");

        embedMock
            .Setup(e => e.GetEmbeddingOptions(It.IsAny<string>()))
            .Returns(new EmbeddingsOptions());

        embedding = new ReadOnlyMemory<float>(TestEmbeddings.CreateEmbeddingArray());
        List<EmbeddingItem> embeddingItems =
        [
            AzureOpenAIModelFactory.EmbeddingItem(embedding)
        ];
        Embeddings embeddings = AzureOpenAIModelFactory.Embeddings(embeddingItems);

        openAIClientMock
            .Setup(client => client.GetEmbeddingsAsync(It.IsAny<EmbeddingsOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embeddingsMock.Object);

        embeddingsMock.SetupGet(embed => embed.Value).Returns(embeddings);

        configMock
            .Setup(config => config[GKeys.AzOpenAIEndpointName])
            .Returns("https://generellem");
        configMock
            .Setup(config => config[GKeys.AzOpenAIApiKey])
            .Returns("generellem-key");
        llmClientFactMock = new(configMock.Object);

        llmClientFactMock.Setup(llm => llm.CreateOpenAIClient()).Returns(openAIClientMock.Object);

        List<TextChunk> chunks =
        [
            new()
            {
                ID = "id1",
                Content = "chunk1",
                DocumentReference = "documentReference1"
            },
            new() 
            {
                ID = "id2",
                Content = "chunk2",
                DocumentReference = "documentReference2"
            }
        ];
        azSearchSvcMock
            .Setup(srchSvc => srchSvc.DoesIndexExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        azSearchSvcMock
            .Setup(srchSvc => srchSvc.GetDocumentReferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);
        azSearchSvcMock
            .Setup(srchSvc => srchSvc.SearchAsync<TextChunk>(It.IsAny<ReadOnlyMemory<float>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);

        azureOpenAIRag = new AzureOpenAIRag();
    }

    //[Fact]
    //public async Task SearchAsync_CallsGetEmbeddingsAsync()
    //{
    //    await azureOpenAIRag.SearchAsync("text", CancellationToken.None);

    //    openAIClientMock.Verify(
    //        client => client.GetEmbeddingsAsync(It.IsAny<EmbeddingsOptions>(), It.IsAny<CancellationToken>()),
    //        Times.Once());
    //}

    //[Fact]
    //public async Task SearchAsync_CallsSearchAsyncWithEmbedding()
    //{
    //    await azureOpenAIRag.SearchAsync("text", CancellationToken.None);

    //    azSearchSvcMock.Verify(srchSvc =>
    //        srchSvc.SearchAsync<TextChunk>(embedding, It.IsAny<CancellationToken>()),
    //        Times.Once());
    //}

    //[Fact]
    //public async Task SearchAsync_ReturnsChunkContents()
    //{
    //    const string ExpectedContent = "chunk1";

    //    var result = await azureOpenAIRag.SearchAsync("text", CancellationToken.None);

    //    Assert.Equal(ExpectedContent, result.First());
    //}

    //[Fact]
    //public async Task SearchAsync_WithRequestFailedExceptionOnGetEmbeddings_LogsAnError()
    //{
    //    openAIClientMock
    //        .Setup(client => client.GetEmbeddingsAsync(It.IsAny<EmbeddingsOptions>(), It.IsAny<CancellationToken>()))
    //        .Throws(new RequestFailedException("Unauthorized"));

    //    await Assert.ThrowsAsync<RequestFailedException>(async () =>
    //        await azureOpenAIRag.SearchAsync("text", CancellationToken.None));

    //    logMock
    //        .Verify(
    //            l => l.Log(
    //                LogLevel.Error,
    //                It.IsAny<EventId>(),
    //                It.IsAny<It.IsAnyType>(),
    //                It.IsAny<Exception>(),
    //                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
    //            Times.Once);
    //}

    //[Fact]
    //public async Task SearchAsync_WithRequestFailedExceptionOnAzSearch_LogsAnError()
    //{
    //    azSearchSvcMock
    //        .Setup(svc => svc.SearchAsync<TextChunk>(It.IsAny<ReadOnlyMemory<float>>(), It.IsAny<CancellationToken>()))
    //        .Throws(new RequestFailedException("Unauthorized"));

    //    await Assert.ThrowsAsync<RequestFailedException>(async () =>
    //        await azureOpenAIRag.SearchAsync("text", CancellationToken.None));

    //    logMock
    //        .Verify(
    //            l => l.Log(
    //                LogLevel.Error,
    //                It.IsAny<EventId>(),
    //                It.IsAny<It.IsAnyType>(),
    //                It.IsAny<Exception>(),
    //                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
    //            Times.Once);
    //}
}
