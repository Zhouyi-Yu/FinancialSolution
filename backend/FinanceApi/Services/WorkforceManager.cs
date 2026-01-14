using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceApi.Services
{
    public enum Skill { Plumbing, Electrical, Carpentry, Masonry }

    public class Worker 
    {
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public List<Skill> Skills { get; set; } = new();
    }

    public class WorkforceManager
    {
        private readonly Dictionary<Skill, List<Worker>> _skillMap;

        public WorkforceManager(IEnumerable<Worker> workers)
        {
            if (workers == null) throw new ArgumentNullException(nameof(workers));

            _skillMap = new Dictionary<Skill, List<Worker>>();
            
            // Initialize all skills to avoid null entry issues
            foreach (Skill skill in Enum.GetValues(typeof(Skill)))
            {
                _skillMap[skill] = workers
                    .Where(w => w.Skills.Contains(skill))
                    .OrderBy(w => w.Rate)
                    .ToList();
            }
        }

        public Worker? GetCheapestWorkerWithSkill(Skill requiredSkill)
        {
            if (_skillMap.TryGetValue(requiredSkill, out var workers) && workers.Count > 0)
            {
                return workers[0];
            }
            return null;
        }
    }
}
