﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YTScrapper.Application.Contracts;
using YTScrapper.Application.DTOs;

namespace YTScrapper.Application.Runners
{
    public class ScraperRunner : IScraperRunner
    {
        private readonly Semaphore _scraperLimiter = new(1, 5);
        private readonly ISearchScrapperCollector _collector;

        public ScraperRunner(ISearchScrapperCollector collector)
        {
            _collector = collector;
        }

        public async Task<List<SearchResult>> Run(SearchRequest searchRequest, CancellationToken token = default)
        {
            var scrappers = await _collector.CollectFor(searchRequest.Websites);
            ConcurrentBag<SearchResult> resultCollection = new();

            try
            {
                var scraperTasks = new List<Task>();
                foreach (var scrapper in scrappers)
                {
                    scraperTasks.Add(Search(searchRequest, resultCollection, scrapper, token));
                }
                await Task.WhenAll(scraperTasks);
            }
            catch (OperationCanceledException)
            {
                //if cancelled - do nothing
            }

            return resultCollection.ToList();
        }

        private async Task Search(SearchRequest searchRequest, ConcurrentBag<SearchResult> resultCollection, ISearchScrapper scrapper, CancellationToken token)
        {
            _scraperLimiter.WaitOne();
            try
            {
                var result = await scrapper.Search(searchRequest, token);
                if (result.HasValue)
                {
                    resultCollection.Add(result.Value);
                }
            }
            finally
            {
                _scraperLimiter.Release();
            }
        }
    }
}