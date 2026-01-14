using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApi.Services;
using Xunit;

namespace FinanceApi.Tests.Services
{
    public class WorkforceManagerTests
    {
        [Fact]
        public void GetCheapestWorker_ShouldReturnCheapest_WhenMultipleWorkersHaveSkill()
        {
            // Arrange (Interview Tip: This is your basic "Success" case)
            var workers = new List<Worker>
            {
                new Worker { Name = "Expensive Bob", Rate = 100, Skills = new List<Skill> { Skill.Plumbing } },
                new Worker { Name = "Cheap Alice", Rate = 50, Skills = new List<Skill> { Skill.Plumbing } },
                new Worker { Name = "Mid Dave", Rate = 75, Skills = new List<Skill> { Skill.Plumbing } }
            };
            var manager = new WorkforceManager(workers);

            // Act
            var result = manager.GetCheapestWorkerWithSkill(Skill.Plumbing);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cheap Alice", result!.Name);
        }

        [Fact]
        public void GetCheapestWorker_ShouldReturnNull_WhenNoWorkerHasSkill()
        {
            // Arrange (Edge Case: No matches)
            var workers = new List<Worker>
            {
                new Worker { Name = "John", Rate = 100, Skills = new List<Skill> { Skill.Carpentry } }
            };
            var manager = new WorkforceManager(workers);

            // Act
            var result = manager.GetCheapestWorkerWithSkill(Skill.Plumbing);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCheapestWorker_ShouldHandleTies_Consistently()
        {
            // Arrange (Edge Case: Tie-breaker)
            var workers = new List<Worker>
            {
                new Worker { Name = "Twin A", Rate = 50, Skills = new List<Skill> { Skill.Electrical } },
                new Worker { Name = "Twin B", Rate = 50, Skills = new List<Skill> { Skill.Electrical } }
            };
            var manager = new WorkforceManager(workers);

            // Act
            var result = manager.GetCheapestWorkerWithSkill(Skill.Electrical);

            // Assert
            Assert.NotNull(result);
            // In a tie, it depends on OrderBy behavior (stable vs unstable)
            // It also tests if your code even crashes on ties.
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenWorkersListIsNull()
        {
            // Arrange (Edge Case: Guard Clauses)
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new WorkforceManager(null!));
        }

        [Fact]
        public void GetCheapestWorker_ShouldWork_WhenWorkerHasMultipleSkills()
        {
            // Arrange (Real-world Case)
            var multiSkilled = new Worker { Name = "Jack of all trades", Rate = 60, Skills = new List<Skill> { Skill.Plumbing, Skill.Electrical } };
            var specialists = new List<Worker> 
            { 
               multiSkilled,
               new Worker { Name = "Pricey Plumber", Rate = 200, Skills = new List<Skill> { Skill.Plumbing } }
            };
            var manager = new WorkforceManager(specialists);

            // Act
            var resultPlumbing = manager.GetCheapestWorkerWithSkill(Skill.Plumbing);
            var resultElectric = manager.GetCheapestWorkerWithSkill(Skill.Electrical);

            // Assert
            Assert.Equal(multiSkilled, resultPlumbing);
            Assert.Equal(multiSkilled, resultElectric);
        }
    }
}
