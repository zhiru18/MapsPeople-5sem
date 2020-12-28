﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Microsoft.Extensions.Hosting;
using Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using MessageBroker;
using System.Linq;

namespace Scheduling {
    public class Scheduler : IHostedService {
        private IDBAccess _dbAccess;
        private IMessageBroker _messageBroker;
        private Timer _timer;
        private Dictionary<string, List<Script>> scriptsSeperetedByLangugage;
        private List<Script> scripts;

        public Scheduler(IDBAccess dBAccess,  IMessageBroker messageBroker) {
            _dbAccess = dBAccess;
            _messageBroker = messageBroker;
            scripts = new List<Script>();
            scriptsSeperetedByLangugage = new Dictionary<string, List<Script>>();
        }
        public Task StartAsync(CancellationToken cancellationToken) {
            _timer = new Timer(
                Run,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5)
                );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;

        }

        private void Run(object state) {
            GetNewScripts();
            SeparateByLanguage();
            try {
                foreach (var scriptList in scriptsSeperetedByLangugage) {
                    SendToRabbitMQ(scriptList.Value);
                }
            } catch (InvalidOperationException) {
                //logging
            } finally {
                ClearLists();
            }
        }

        private void SeparateByLanguage() {
            foreach (var script in scripts) {
                if (scriptsSeperetedByLangugage.ContainsKey(script.Language)) {
                    List<Script> scriptList;
                    scriptsSeperetedByLangugage.TryGetValue(script.Language, out scriptList);
                    List<Script> temp = new List<Script>(scriptList);
                    temp.Add(script);
                    scriptsSeperetedByLangugage[script.Language] = temp.Distinct().ToList();
                } else {
                    List<Script> scriptList = new List<Script>();
                    scriptList.Add(script);
                    scriptsSeperetedByLangugage.Add(script.Language, scriptList);
                }
                
            }
        }


        private IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize) {
            for (int i = 0; i < items.Count; i += nSize) {
                yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
            }
        }

        private void SendToRabbitMQ(List<Script> scripts) {
            var scriptLists = SplitList<Script>(scripts, int.Parse(Environment.GetEnvironmentVariable("MP_CHUNKSIZE")));
            string queueName = Environment.GetEnvironmentVariable("MP_SCHEDULINGQUEUE");
            foreach (var list in scriptLists) {
                _messageBroker.Send<Script>(queueName, scripts);
            }
        }

        private void GetNewScripts() {
            scripts = _dbAccess.GetAll().ToList();
        }

        private void ClearLists() {
            foreach (var scriptList in scriptsSeperetedByLangugage) {
                scriptList.Value.Clear();
            }
        }
    }
}