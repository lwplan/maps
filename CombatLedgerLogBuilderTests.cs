// extern alias enginecore;
// using System;
// using System.Collections.Generic;
// using System.Collections.ObjectModel;
// using System.IO;
// using System.Linq;
// using NUnit.Framework;
// using CombatEngine.Abilities.Definitions;
// using CombatEngine.Resolution;
// using EngineCombatCharacter = enginecore::CombatEngine.Core.CombatCharacter;
// using EngineCombatContext = enginecore::CombatEngine.Core.CombatContext;
// using EngineGuidT = enginecore::CombatEngine.Core.GuidT;
// using EngineIHexPiece = enginecore::CombatEngine.Core.IHexPiece;
//
// namespace MyProject.Tests.Server.Combat
// {
//     public class CombatLedgerLogBuilderTests
//     {
//         private static readonly EngineGuidT PlayerGuid = EngineGuidT.Parse("11111111-1111-1111-1111-111111111111");
//         private static readonly EngineGuidT TargetGuid = EngineGuidT.Parse("22222222-2222-2222-2222-222222222222");
//
//         [Test]
//         public void Build_WithSnapshot_ConstructsExpectedHierarchy()
//         {
//             var builder = new CombatLedgerLogBuilder();
//             var ledgerSnapshot = CreateSnapshot();
//
//             var result = builder.Build(ledgerSnapshot);
//
//             Assert.Multiple(() =>
//             {
//                 Assert.That(result.Title, Is.EqualTo("Combat Ledger"));
//                 Assert.That(result.Summary, Is.EqualTo("2 transaction(s)"));
//                 Assert.That(result.Children, Has.Count.EqualTo(2));
//
//                 var first = result.Children[0];
//                 Assert.That(first.Title, Is.EqualTo("Transaction 1"));
//                 Assert.That(first.Summary, Is.EqualTo("Ability (Sword Slash) dealt 25 Fire to Target Dummy"));
//                 Assert.That(first.Children.Select(child => child.Title), Is.EqualTo(new[]
//                 {
//                     "Step 1"
//                 }));
//
//                 var actionNode = first.Children.First().Children.First();
//                 Assert.That(actionNode.Title, Is.EqualTo("Sword Slash"));
//                 Assert.That(actionNode.Summary, Is.EqualTo("On Target Dummy | Damage: Ability (Sword Slash) dealt 25 Fire to Target Dummy"));
//
//                 var second = result.Children[1];
//                 Assert.That(second.Title, Is.EqualTo("Transaction 2"));
//                 Assert.That(second.Summary, Is.EqualTo("No damage dealt"));
//                 Assert.That(second.Children.Select(child => child.Title), Is.EqualTo(new[]
//                 {
//                     "No steps"
//                 }));
//             });
//         }
//
//         [Test]
//         public void Format_WithSnapshot_MatchesGoldenOutput()
//         {
//             var builder = new CombatLedgerLogBuilder();
//             var formatter = new HierarchicalLogTextFormatter();
//             var ledgerSnapshot = CreateSnapshot();
//
//             var hierarchy = builder.Build(ledgerSnapshot);
//             var formatted = Normalize(formatter.Format(hierarchy));
//
//             var goldenPath = Path.Combine(
//                 TestContext.CurrentContext.TestDirectory,
//                 "Fixtures",
//                 "Combat",
//                 "ledger-log.txt");
//
//             var expected = Normalize(File.ReadAllText(goldenPath));
//
//             TestContext.WriteLine(formatted);
//             Assert.That(formatted, Is.EqualTo(expected));
//         }
//
//         [Test]
//         public void Format_WithPlayedIntentSnapshot_MatchesGoldenOutput()
//         {
//             var builder = new CombatLedgerLogBuilder();
//             var formatter = new HierarchicalLogTextFormatter();
//             var ledgerSnapshot = CreatePlayedIntentSnapshot();
//             var combatContext = CreateCombatContext();
//
//             var hierarchy = builder.Build(ledgerSnapshot, combatContext);
//             var formatted = Normalize(formatter.Format(hierarchy));
//
//             var goldenPath = Path.Combine(
//                 TestContext.CurrentContext.TestDirectory,
//                 "Fixtures",
//                 "Combat",
//                 "ledger-log-played.txt");
//
//             var expected = Normalize(File.ReadAllText(goldenPath));
//
//             TestContext.WriteLine(formatted);
//             Assert.That(formatted, Is.EqualTo(expected));
//         }
//
//         [Test]
//         public void Build_WithPlayedIntentBusEvent_UsesPlayerSummary()
//         {
//             var builder = new CombatLedgerLogBuilder();
//             var combatContext = CreateCombatContext();
//             var ledgerSnapshot = CreatePlayedIntentSnapshot();
//
//             var result = builder.Build(ledgerSnapshot, combatContext);
//
//             var playedEntry = result.Children.First();
//
//             Assert.That(
//                 playedEntry.Summary,
//                 Is.EqualTo(
//                     "Archer Played Flame Burst : dealt 30 Fire to Goblin, restored 10 hp to Archer, applied Burn to Goblin, paid 5 Mana."));
//         }
//
//         private static CombatTransactionLedgerSnapshot CreateSnapshot()
//         {
//             var mage = new IntentSourceReference("source-1", "Mage", "Player");
//             var slash = new IntentSourceReference("source-2", "Sword Slash", "Ability");
//             var shield = new IntentSourceReference("source-3", "Shield Block", "Ability");
//
//             var entryOne = new CombatTransactionLogEntrySnapshot(
//                 1,
//                 new CombatTransactionSnapshot("IntentSourcing", 1, 2, mage, null, null, null),
//                 new ReadOnlyCollection<CombatEventSnapshot>(new List<CombatEventSnapshot>
//                 {
//                     new CombatEventSnapshot("target-1", "SlashAction")
//                 }),
//                 new ReadOnlyCollection<IntentSourceReference>(new List<IntentSourceReference> { slash }),
//                 mage,
//                 null,
//                 null,
//                 new ReadOnlyCollection<CombatDamageSummarySnapshot>(new List<CombatDamageSummarySnapshot>
//                 {
//                     new CombatDamageSummarySnapshot(
//                         25,
//                         "Fire",
//                         "target-1",
//                         "Target Dummy",
//                         mage,
//                         new ReadOnlyCollection<IntentSourceReference>(new List<IntentSourceReference> { slash }),
//                         slash)
//                 }));
//
//             var entryTwo = new CombatTransactionLogEntrySnapshot(
//                 2,
//                 new CombatTransactionSnapshot("FinalizeTransaction", 2, 3, shield, null, null, null),
//                 new ReadOnlyCollection<CombatEventSnapshot>(Array.Empty<CombatEventSnapshot>()),
//                 new ReadOnlyCollection<IntentSourceReference>(Array.Empty<IntentSourceReference>()),
//                 shield,
//                 null,
//                 null,
//                 new ReadOnlyCollection<CombatDamageSummarySnapshot>(Array.Empty<CombatDamageSummarySnapshot>()));
//
//             var traceOne = new IntentTraceNodeSnapshot(
//                 "trace-1",
//                 slash,
//                 null,
//                 new ReadOnlyCollection<string>(new List<string> { "Intent" }),
//                 new ReadOnlyCollection<string>(new List<string> { "trace-2" }),
//                 1,
//                 "IntentSourcing");
//
//             var traceTwo = new IntentTraceNodeSnapshot(
//                 "trace-2",
//                 mage,
//                 "trace-1",
//                 new ReadOnlyCollection<string>(new List<string> { "Cause" }),
//                 new ReadOnlyCollection<string>(Array.Empty<string>()),
//                 1,
//                 "IntentSourcing");
//
//             return new CombatTransactionLedgerSnapshot(
//                 new ReadOnlyCollection<CombatTransactionLogEntrySnapshot>(new List<CombatTransactionLogEntrySnapshot>
//                 {
//                     entryOne,
//                     entryTwo
//                 }),
//                 new ReadOnlyCollection<IntentTraceNodeSnapshot>(new List<IntentTraceNodeSnapshot>
//                 {
//                     traceOne,
//                     traceTwo
//                 }));
//         }
//
//         private static CombatTransactionLedgerSnapshot CreatePlayedIntentSnapshot()
//         {
//             var playerId = PlayerGuid.ToString();
//             var targetId = TargetGuid.ToString();
//             var abilitySource = new IntentSourceReference("source-ability", "Flame Burst", "Ability");
//
//             var busEvent = new IntentBusEventSnapshot(
//                 "bus-1",
//                 "PlayedIntentBusEvent",
//                 "PlayerAction",
//                 null,
//                 new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
//                 {
//                     [Anchor.Player.ToString()] = playerId,
//                     [Anchor.SelectedTarget.ToString()] = targetId
//                 }));
//
//             var transactionSnapshot = new CombatTransactionSnapshot(
//                 "IntentSourcing",
//                 1,
//                 1,
//                 abilitySource,
//                 busEvent,
//                 null,
//                 null);
//
//             var healEvent = new CombatEventSnapshot(
//                 playerId,
//                 "HealAction",
//                 HpDelta: 10);
//
//             var eotEvent = new CombatEventSnapshot(
//                 targetId,
//                 "ApplyEoT",
//                 EoTKey: "Burn",
//                 EoTDurationKind: DurationKind.Turns,
//                 EoTDurationValue: 2);
//
//             var events = new ReadOnlyCollection<CombatEventSnapshot>(new List<CombatEventSnapshot>
//             {
//                 new CombatEventSnapshot(targetId, "DealAction"),
//                 healEvent,
//                 eotEvent
//             });
//
//             var damageSummaries = new ReadOnlyCollection<CombatDamageSummarySnapshot>(new List<CombatDamageSummarySnapshot>
//             {
//                 new CombatDamageSummarySnapshot(
//                     30,
//                     "Fire",
//                     targetId,
//                     "Goblin",
//                     abilitySource,
//                     new ReadOnlyCollection<IntentSourceReference>(new List<IntentSourceReference> { abilitySource }),
//                     abilitySource)
//             });
//
//             var entry = new CombatTransactionLogEntrySnapshot(
//                 1,
//                 transactionSnapshot,
//                 events,
//                 new ReadOnlyCollection<IntentSourceReference>(new List<IntentSourceReference> { abilitySource }),
//                 abilitySource,
//                 new IntentSourceReference("player-effect", "Archer", "PlayedIntentBusEvent"),
//                 new HexPieceReference("hex-1", "Flame Burst", "Fire", "Rare", true, false, 5),
//                 damageSummaries);
//
//             return new CombatTransactionLedgerSnapshot(
//                 new ReadOnlyCollection<CombatTransactionLogEntrySnapshot>(new List<CombatTransactionLogEntrySnapshot> { entry }),
//                 new ReadOnlyCollection<IntentTraceNodeSnapshot>(Array.Empty<IntentTraceNodeSnapshot>()));
//         }
//
//         private static EngineCombatContext CreateCombatContext()
//         {
//             var characters = new List<EngineCombatCharacter>();
//
//             characters.Add(new EngineCombatCharacter(PlayerGuid, true)
//             {
//                 Name = "Archer"
//             });
//
//             characters.Add(new EngineCombatCharacter(TargetGuid, false)
//             {
//                 Name = "Goblin"
//             });
//
//             return new EngineCombatContext(_ => (EngineIHexPiece?)null, characters);
//         }
//
//         private static string Normalize(string value)
//         {
//             return value.Replace("\r\n", "\n").TrimEnd();
//         }
//     }
// }
