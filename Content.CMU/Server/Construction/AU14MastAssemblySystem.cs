using Content.Server.Construction;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.Repairable;
using Content.Shared.CMU14.Construction;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Content.Shared.Tools.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.CMU14.Construction;

/// <summary>
///     Skill gate for the field antenna mast assembly. The construction graph in comms.yml owns the steps,
///     the materials and the timings, but graph steps never see the user, so training has to be enforced one
///     level up: an interaction that looks like construction work is vetoed here before
///     <see cref="ConstructionSystem"/> gets a chance to validate it. That keeps a footing someone paid 40
///     sheets for from being finished - or a working mast quietly dismantled - by anybody who happens to be
///     holding a wrench.
/// </summary>
public sealed partial class AU14MastAssemblySystem : EntitySystem
{
    [Dependency] private SkillsSystem _skills = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private TagSystem _tags = default!;

    private static readonly ProtoId<TagPrototype> AntennaHeadTag = "AU14MastAntennaHead";

    public override void Initialize()
    {
        base.Initialize();

        // After repair so welding a damaged mast is still a repair rather than a training complaint,
        // before construction so an untrained user never reaches a graph step.
        SubscribeLocalEvent<AU14MastAssemblyComponent, InteractUsingEvent>(OnInteractUsing,
            before: [typeof(ConstructionSystem)],
            after: [typeof(RMCRepairableSystem)]);
        SubscribeLocalEvent<AU14MastAssemblyComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInteractUsing(Entity<AU14MastAssemblyComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Only stand in the way of things that could actually be a construction step. A finished mast is also
        // a splice target and a repair target, and swallowing every interaction here would break both.
        if (!IsConstructionInput(args.Used))
            return;

        if (_skills.HasSkill(args.User, ent.Comp.Skill, ent.Comp.RequiredSkillLevel))
            return;

        // Handled, so construction never sees the interaction and the untrained user is told why rather than
        // being left clicking at a structure that silently does nothing.
        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("au14-mast-untrained"), ent, args.User, PopupType.SmallCaution);
    }

    /// <summary>Whether this item is the sort of thing the mast graph consumes: a tool, a material stack or the antenna head.</summary>
    private bool IsConstructionInput(EntityUid used)
    {
        return HasComp<ToolComponent>(used) ||
               HasComp<StackComponent>(used) ||
               _tags.HasTag(used, AntennaHeadTag);
    }

    private void OnExamined(Entity<AU14MastAssemblyComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ExamineHint)
            return;

        if (!_skills.HasSkill(args.Examiner, ent.Comp.Skill, ent.Comp.RequiredSkillLevel))
            args.PushMarkup(Loc.GetString("au14-mast-examine-untrained"));
    }
}
