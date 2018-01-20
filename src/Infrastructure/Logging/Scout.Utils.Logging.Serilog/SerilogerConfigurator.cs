using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Scout.Utils.Logging.Serilog
{
    /// <summary>
    /// Дефолтная реализация ISerilogConfigurator,
    /// позваляющая из коробки писать в файлы, Elasticsearch и консоль. 
    /// </summary>
    /// <remarks>Если требуется более тонкая настройка следует самомму реализовать ISerilogConfigurator</remarks>
    public class SerilogerConfigurator : ISerilogConfigurator
    {
        private readonly List<ElasticsearchSinkOptions> _elasticsearchOptions;
        private readonly List<FileSinkOptions> _fileSinkOptions;
        
        private bool _isConsoneEnabled;
        private LogEventLevel _consoleMinLevel;

        public SerilogerConfigurator()
        {
            _elasticsearchOptions = new List<ElasticsearchSinkOptions>();
            _fileSinkOptions = new List<FileSinkOptions>();
            
            _isConsoneEnabled = false;
            _consoleMinLevel = LogEventLevel.Fatal;
        }

        /// <summary>
        /// Метод вызывается при создании логера в SeriloggerFactory
        /// </summary>
        /// <param name="configuration"></param>
        public void ConfigureSerilog(LoggerConfiguration configuration)
        {
            foreach (var elasticsearchOption in _elasticsearchOptions ?? new List<ElasticsearchSinkOptions>(0))
            {
                configuration.WriteTo.Elasticsearch(elasticsearchOption);
            }

            foreach (var fileSinkOption in _fileSinkOptions ?? new List<FileSinkOptions>(0))
            {
                configuration
                    .WriteTo
                    .File(fileSinkOption.Path, fileSinkOption.MinLevel, rollingInterval: fileSinkOption.Interval, shared: true);
            }

            if (_isConsoneEnabled)
            {
                configuration
                    .WriteTo
                    .Console(_consoleMinLevel);
            }
        }

        /// <summary>
        /// Путь к файлу с конфигурацией
        /// </summary>
        public string ConfigPath { get; set; }

        /// <summary>
        /// Добавить настройку для записи в Elasticsearch
        /// </summary>
        /// <param name="elasticEndpoint"> Адрес Elasticsearch</param>
        /// <param name="minLevel"> минимальный уровень </param>
        /// <param name="indexBaseName">База имени индекса. 
        /// Полное имя формируется как: [базовое имя]-[текущая дата в формате yyyy.MM.dd].
        /// Дефолтное имя: "logstash-{0:yyyy.MM.dd}"
        /// </param>
        public SerilogerConfigurator AddElasticSearchTarget(Uri elasticEndpoint, LogEventLevel minLevel, string indexBaseName = null)
        {
            var option = new ElasticsearchSinkOptions(elasticEndpoint);
            option.MinimumLogEventLevel = minLevel;

            if (!string.IsNullOrWhiteSpace(indexBaseName))
            {
                option.IndexFormat = indexBaseName + "-{0:yyyy.MM}";
            }
            
            _elasticsearchOptions.Add(option);
            
            return this;
        }

        /// <summary>
        /// Добавить таргет для записи в файл
        /// </summary>
        /// <param name="path">Относительный или абсолютный путь к файлу (дата к имени файла будет добавлена автоматически)</param>
        /// <param name="minLevel"> Мин. уровень</param>
        /// <param name="interval">Интервал обновления файла </param>
        public SerilogerConfigurator AddFileTarget(string path, LogEventLevel minLevel, RollingInterval interval = RollingInterval.Day)
        {
            if(string.IsNullOrWhiteSpace(path)) return this;
            
            _fileSinkOptions.Add(new FileSinkOptions()
            {
                Path = path,
                MinLevel = minLevel, 
                Interval = interval
            });

            return this;
        }

        /// <summary>
        /// Активировать запись в консоль
        /// </summary>
        /// <param name="minLevel"></param>
        public SerilogerConfigurator EnableConsoleOutput(LogEventLevel minLevel)
        {
            _isConsoneEnabled = true;
            _consoleMinLevel = minLevel;

            return this;
        }
        
        private class FileSinkOptions
        {
            public string Path { get; set; }
            public LogEventLevel MinLevel { get; set; }
            public RollingInterval Interval { get; set; }
        }
    }
}