﻿using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using YTSearch.Application.Contracts;
using YTSearch.Domain.Models;

namespace YTSearch.Infrastructure.Repository
{
    public class YouTubeRepository : ISearchItemRepository
    {
        private readonly string _connectionString;

        public YouTubeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlLiteDefault");
        }

        public async Task<int> Add(YouTubeModel searchItem)
        {
            var sql = @"
insert into searchitem 
(""ImagePreviewUrl"", ""Url"", ""Title"", ""Description"", ""Author"", ""Duration"")
values(@preview, @url, @title, @description, @author, @duration) RETURNING Id;";

            using IDbConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var output = await connection.QueryAsync<int>(sql, new {
                preview = searchItem.ImagePreviewUrl,
                url = searchItem.Url,
                title = searchItem.Title,
                description = searchItem.Description,
                author = searchItem.Author,
                duration = searchItem.Duration,
            });

            return output.FirstOrDefault();
        }

        public async Task Delete(YouTubeModel searchItem)
        {
            var sql = @"
delete from searchitem
where id = @id;";

            using IDbConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var output = await connection.QueryAsync<YouTubeModel>(sql, new
            {
                id = searchItem.Id,
            });
        }

        public async Task<List<YouTubeModel>> Get()
        {
            using IDbConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var output = await connection.QueryAsync<YouTubeModel>(@"select * from searchitem;");
            return output.AsList();
        }

        public async Task Update(YouTubeModel searchItem)
        {
            var sql = @"
UPDATE searchitem
SET ImagePreviewUrl = @preview, Url= @url, Title=@title, Description=@description, Author=@author, Duration=@duration
WHERE Id = @id;";

            using IDbConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var output = await connection.QueryAsync<YouTubeModel>(sql, new
            {
                id = searchItem.Id,
                preview = searchItem.ImagePreviewUrl,
                url = searchItem.Url,
                title = searchItem.Title,
                description = searchItem.Description,
                author = searchItem.Author,
                duration = searchItem.Duration,
            });
        }
    }
}
