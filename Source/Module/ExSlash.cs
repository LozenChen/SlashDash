using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SlashDash.Module;

[Tracked(false)]
[Pooled]
public class ExSlash : Entity {

    public Sprite Sprite;

    public Vector2 Direction;

    public float Delay;

    public float MovingSpeed;

    public float NextGenerationPossibility;

    public int Generation;

    public Vector2 BasePosition;

    public int BaseDashCount;

    public static int LivingSlashes = 0;

    public static int MaxCapacity = 4000; // don't set it above 20000

    public static float MinDelay = 0.05f;

    public static float MaxDelay = 0.7f;
    public ExSlash() {
        Add(Sprite = new Sprite(GFX.Game, "effects/slash/"));
        Sprite.Add("play", "", 0.1f, 0, 1, 2, 3);
        Sprite.CenterOrigin();
        Sprite.OnFinish = delegate {
            RemoveSelf();
        };
        base.Depth = 200; // deeper than main slash, player and theoCrystal
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        if (LivingSlashes > 0) {
            LivingSlashes--;
            // in case LivingSlashes become negative after transition
        }
    }

    public override void Update() {
        if (Delay > 0f) {
            Delay -= Engine.DeltaTime;
            if (Delay <= 0f) {
                Sprite.Play("play", restart: true);
                Visible = true;
            }
        }
        else {
            Position += Direction * MovingSpeed * Engine.DeltaTime;
            base.Update();
        }
    }

    public static ExSlash Burst(Vector2 basePosition, Vector2 position, float direction, Color color, float length = 1f, float delay = 0f, float movingSpeed = 8f) {
        ExSlash slashFx = Engine.Pooler.Create<ExSlash>();
        slashFx.BaseDashCount = DashCount;
        LivingSlashes++;
        Engine.Scene.Add(slashFx);
        slashFx.BasePosition = basePosition;
        slashFx.Position = position;
        slashFx.Direction = Calc.AngleToVector(direction, length);
        if (delay <= 0f) {
            slashFx.Sprite.Play("play", restart: true);
            slashFx.Visible = true;
        }
        else {
            slashFx.Delay = delay;
            slashFx.Visible = false;
        }
        slashFx.Sprite.Scale = new Vector2(length, 1f);
        slashFx.Sprite.Rotation = 0f;
        slashFx.MovingSpeed = movingSpeed;
        if (Math.Abs(direction - MathF.PI) > 0.01f) {
            slashFx.Sprite.Rotation = direction;
        }
        slashFx.Active = true;
        slashFx.Sprite.Color = color;

        return slashFx;
    }

    public static float BaseGenerationRate;

    public static int MaxNextGenerationCount;

    public static int DashCount;

    public static void ExplosiveRandBurst(Vector2 position, float range) {
        int tryCount = MaxNextGenerationCount;

        for (int i = 1; i <= tryCount; i++) {
            if (Rand.Rnd.Chance(BaseGenerationRate)) {
                if (TryRandBurst(position, position, range, out ExSlash slash)) {
                    slash.Generation = 1;
                    slash.NextGenerationPossibility = BaseGenerationRate * BaseGenerationRate;
                    SetGeneration(slash);
                }
                else {
                    break;
                }
            }
        }

    }

    public static bool TryRandBurst(Vector2 basePosition, Vector2 position, float rangeOrLerp, out ExSlash slash) {
        if (LivingSlashes > MaxCapacity) {
            slash = null;
            return false;
        }

        // if var = rangeOrLerp < 1f, then var is the lerp
        // otherwise, var is the range of random offset

        Vector2 newPosition = rangeOrLerp > 1f ? position + Rand.Rnd.GetDirection(Rand.Rnd.NextFloat(5f * rangeOrLerp)) : Vector2.Lerp(basePosition, position, rangeOrLerp) + Rand.Rnd.GetDirection(Rand.Rnd.NextFloat(200f * rangeOrLerp));

        slash = Burst(basePosition, newPosition, Rand.Rnd.Range(-MathF.PI, MathF.PI), Rand.GetColor(), Rand.Rnd.Range(0.5f, 6f), Rand.Rnd.Range(MinDelay, MaxDelay), Rand.Rnd.Range(3f, 120f));
        return true;
    }

    public static void SetGeneration(ExSlash slash) {
        slash.Sprite.OnFinish += (_) => {
            slash.GenerateNext();
        };
    }

    public void GenerateNext() {
        if (Generation <= 10) {
            if (LivingSlashes > MaxNextGenerationCount - 100 && DashCount > BaseDashCount) {
                // if player dashes again, then make previous slashes disappear as soon as possible so we can create new slashes
                return;
            }
            int tryCount = MaxNextGenerationCount;
            float localizer = (Position - BasePosition).LengthSquared() > 1E4 * Generation ? Generation / 10f : MovingSpeed;
            // limit its spread speed, but don't limit too much (otherwise too many slashes on screen)
            // so they look like a spreading-out storm
            for (int i = 1; i <= tryCount; i++) {
                if (Rand.Rnd.Chance(NextGenerationPossibility)) {
                    if (TryRandBurst(BasePosition, Position, localizer, out ExSlash nextGeneration)) {
                        nextGeneration.NextGenerationPossibility = NextGenerationPossibility * BaseGenerationRate;
                        nextGeneration.Generation = Generation + 1;
                        SetGeneration(nextGeneration);
                    }
                    else {
                        break;
                    }
                }
            }
        }
        else {
            // sorry but i will kill you
        }
    }
}

internal static class Rand {
    public static Random Rnd = new Random();

    public static void SetSeed(int seed) {
        Rnd = new Random(seed);
    }

    public static Color GetColor(int whiteness = 255) {
        int r = Rnd.Range(0, 255);
        int g = Rnd.Range(0, 255);
        int b = Rnd.Range(0, 255);
        if (r + g + b < whiteness) {
            r = 255 - r;
            g = 255 - g;
            b = 255 - b;
        }
        return new Color(r, g, b);
    }

    public static Vector2 GetDirection(this Random rand, float length = 1f) {
        return Calc.AngleToVector(rand.NextFloat() * (MathF.PI * 2f), length);
    }
}