using System;
using System.Linq;
using Content.Server.CMU14.Radio;
using Content.Server.Station.Systems;
using Content.Shared.CMU14.CCVar;
using Content.Shared.CMU14.Radio;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Radio;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.CMU14.Radio;

// the panel's VOL rocker is a real switch on the set, not a slider on a window: MUTE kills the
// pack's speaker outright and anything above LOW is loud enough for the people standing around
// the operator to hear the net. these cover the detents meaning what the panel says they mean
[TestFixture]
public sealed class ANPRCSpeakerVolumeTest
{
    private static readonly EntProtoId Pack = "ANPRC117GRadioFilled";
    private static readonly EntProtoId Headset = "AU14HeadsetGovforCommand";
    private static readonly ProtoId<JobPrototype> Rifleman = "AU14JobGOVFORSquadRifleman";
    private static readonly ProtoId<RadioChannelPrototype> Channel = "radioGovforCommand";

    private const string BackSlot = "back";
    private const string EarsSlot = "ears";

    [Test]
    public void SpeakerRangeFollowsTheDetents()
    {
        var radio = new ANPRCRadioComponent();

        // MUTE and LOW never leave the operator. every detent above that is another two
        // tiles of bystander, which is the whole reason the rocker is worth touching
        radio.Volume = 0;
        Assert.That(radio.SpeakerBleedRange, Is.EqualTo(0f));

        radio.Volume = ANPRCRadioComponent.DefaultVolume;
        Assert.That(radio.SpeakerBleedRange, Is.EqualTo(0f));

        radio.Volume = ANPRCRadioComponent.DefaultVolume + 1;
        Assert.That(radio.SpeakerBleedRange, Is.GreaterThan(0f));

        radio.Volume = ANPRCRadioComponent.MaxVolume;
        Assert.That(
            radio.SpeakerBleedRange,
            Is.GreaterThan(new ANPRCRadioComponent { Volume = ANPRCRadioComponent.MaxVolume - 1 }.SpeakerBleedRange));
    }

    [Test]
    public async Task LoudSpeakerIsHeardByBystandersAndQuietOneIsNot()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();

        var config = server.ResolveDependency<IConfigurationManager>();
        var wasEnabled = config.GetCVar(AU14CCVars.NewCommsSystem);

        await server.WaitPost(() => config.SetCVar(AU14CCVars.NewCommsSystem, true));

        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            var spawning = server.System<StationSpawningSystem>();
            var inventory = server.System<InventorySystem>();
            var radios = server.System<ANPRCRadioSystem>();

            var wearer = spawning.SpawnPlayerMob(
                testMap.GridCoords,
                Rifleman,
                new HumanoidCharacterProfile(),
                station: null);

            var bystander = spawning.SpawnPlayerMob(
                testMap.GridCoords,
                Rifleman,
                new HumanoidCharacterProfile(),
                station: null);

            var pack = entities.SpawnEntity(Pack, testMap.GridCoords);

            try
            {
                if (inventory.TryGetSlotEntity(wearer, BackSlot, out var oldBack))
                    entities.DeleteEntity(oldBack.Value);

                Assert.That(inventory.TryEquip(wearer, pack, BackSlot, force: true), Is.True, BackSlot);

                var radio = entities.GetComponent<ANPRCRadioComponent>(pack);
                var set = new Entity<ANPRCRadioComponent>(pack, radio);

                // at the detent the set ships on, the speaker is the operator's own ear
                radio.Volume = ANPRCRadioComponent.DefaultVolume;

                Assert.That(
                    radios.GetSpeakerAudience(set, wearer, Channel.Id, EntityUid.Invalid),
                    Is.Empty,
                    "a speaker at LOW reached somebody other than the operator");

                // wound up, the man standing next to the operator is reading the net too
                radio.Volume = ANPRCRadioComponent.MaxVolume;

                var audience = radios.GetSpeakerAudience(set, wearer, Channel.Id, EntityUid.Invalid);

                Assert.That(audience, Does.Contain(bystander), "a speaker at MAX was not heard by a bystander");
                Assert.That(audience, Does.Not.Contain(wearer), "the operator was counted as their own bystander");
            }
            finally
            {
                entities.DeleteEntity(wearer);
                entities.DeleteEntity(bystander);
                entities.DeleteEntity(pack);
            }
        });

        await server.WaitPost(() => config.SetCVar(AU14CCVars.NewCommsSystem, wasEnabled));
        await pair.CleanReturnAsync();
    }

    // a man wearing a headset that already carries the net hears it from his own gear. the pack's
    // receiver lives on the pack rather than on its wearer, so the naive "does this listener have
    // an ActiveRadio for the channel" test misses both headsets and other people's manpacks, and
    // the speaker said the same line to them twice
    [Test]
    public async Task SpeakerSkipsAnybodyAlreadyCarryingTheNet()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();

        var config = server.ResolveDependency<IConfigurationManager>();
        var wasEnabled = config.GetCVar(AU14CCVars.NewCommsSystem);

        await server.WaitPost(() => config.SetCVar(AU14CCVars.NewCommsSystem, true));

        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            var spawning = server.System<StationSpawningSystem>();
            var inventory = server.System<InventorySystem>();
            var radios = server.System<ANPRCRadioSystem>();

            var wearer = spawning.SpawnPlayerMob(
                testMap.GridCoords,
                Rifleman,
                new HumanoidCharacterProfile(),
                station: null);

            var bystander = spawning.SpawnPlayerMob(
                testMap.GridCoords,
                Rifleman,
                new HumanoidCharacterProfile(),
                station: null);

            var pack = entities.SpawnEntity(Pack, testMap.GridCoords);
            var headset = entities.SpawnEntity(Headset, testMap.GridCoords);

            try
            {
                if (inventory.TryGetSlotEntity(wearer, BackSlot, out var oldBack))
                    entities.DeleteEntity(oldBack.Value);

                Assert.That(inventory.TryEquip(wearer, pack, BackSlot, force: true), Is.True, BackSlot);

                var radio = entities.GetComponent<ANPRCRadioComponent>(pack);
                radio.Volume = ANPRCRadioComponent.MaxVolume;

                var set = new Entity<ANPRCRadioComponent>(pack, radio);

                // bare-eared, he is in the speaker's earshot
                if (inventory.TryGetSlotEntity(bystander, EarsSlot, out var oldEars))
                    entities.DeleteEntity(oldEars.Value);

                Assert.That(
                    radios.GetSpeakerAudience(set, wearer, Channel.Id, EntityUid.Invalid),
                    Does.Contain(bystander),
                    "a bystander with nothing in his ears did not hear the speaker");

                // holding the key himself, the net reaches him through his own headset and the
                // speaker must not say it again
                Assert.That(inventory.TryEquip(bystander, headset, EarsSlot, force: true), Is.True, EarsSlot);

                Assert.That(
                    radios.GetSpeakerAudience(set, wearer, Channel.Id, EntityUid.Invalid),
                    Does.Not.Contain(bystander),
                    "the speaker doubled the net up on somebody whose headset already carries it");
            }
            finally
            {
                entities.DeleteEntity(wearer);
                entities.DeleteEntity(bystander);
                entities.DeleteEntity(pack);
                entities.DeleteEntity(headset);
            }
        });

        await server.WaitPost(() => config.SetCVar(AU14CCVars.NewCommsSystem, wasEnabled));
        await pair.CleanReturnAsync();
    }

    // a dead speaker is not a deaf set. the log is what the panel reads back and what gets
    // printed, and muting the audio must not quietly stop the radio writing traffic down
    [Test]
    public async Task MutedSetStillLogsTrafficAndLightsItsReceiveLamp()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();

        var config = server.ResolveDependency<IConfigurationManager>();
        var wasEnabled = config.GetCVar(AU14CCVars.NewCommsSystem);

        await server.WaitPost(() => config.SetCVar(AU14CCVars.NewCommsSystem, true));

        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            var station = entities.SpawnEntity(Pack, testMap.GridCoords);

            try
            {
                var radio = entities.GetComponent<ANPRCRadioComponent>(station);
                radio.Planted = true;
                radio.Volume = 0;

                Assert.That(radio.LastReceive, Is.EqualTo(TimeSpan.Zero));

                var traffic = new ANPRCDirectTrafficReceivedEvent(
                    EntityUid.Invalid,
                    "GIBBS",
                    RadioFrequency.FromKilohertz(146_900),
                    "contact west",
                    "English");

                entities.EventBus.RaiseLocalEvent(station, ref traffic);

                Assert.That(radio.NetLog, Has.Count.EqualTo(1), "a muted set stopped writing traffic down");
                Assert.That(radio.NetLog.Single().Message, Is.EqualTo("contact west"));
                Assert.That(radio.LastReceive, Is.GreaterThan(TimeSpan.Zero), "the receive lamp was never stamped");
            }
            finally
            {
                entities.DeleteEntity(station);
            }
        });

        await server.WaitPost(() => config.SetCVar(AU14CCVars.NewCommsSystem, wasEnabled));
        await pair.CleanReturnAsync();
    }
}
