using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class FlockManager : MonoBehaviour
{
    public int numCreatures = 10000;
    public float minSpeed = 30f;
    public float maxSpeed = 50f;

    public Vector3 worldBounds = new Vector3(30f, 20f, 30f);

    public bool flockCentering = true;
    public bool velocityMatching = true;
    public bool collisionAvoidance = true;
    public bool wandering = true;

    public float centeringWeight = 1f;
    public float velocityWeight = 1.4f;
    public float avoidanceWeight = 1.7f;
    public float wanderingWeight = 1f;

    public float neighborRadius = 5f;
    public float avoidanceRadius = 2f;

    public bool trails = false;

    // Card spin speed (degrees per second)
    public float cardSpinSpeed = 45f;

    private List<Creature> creatures = new List<Creature>();
    private int creatureCount;
    private GameObject creaturePrefab;

    // Pre-baked card textures: one per suit (Hearts, Diamonds, Clubs, Spades)
    private Texture2D[] cardTextures;

    // Suit symbols and colors
    private static readonly string[] suits     = { "♥", "♦", "♣", "♠" };
    private static readonly Color[]  suitColors = {
        new Color(0.85f, 0.1f, 0.1f),   // Hearts   – red
        new Color(0.85f, 0.1f, 0.1f),   // Diamonds – red
        new Color(0.1f,  0.1f, 0.1f),   // Clubs    – black
        new Color(0.1f,  0.1f, 0.1f),   // Spades   – black
    };
    private static readonly string[] ranks = {
        "A","2","3","4","5","6","7","8","9","10","J","Q","K"
    };

    // ---------------------------------------------------------------
    void Start()
    {
        BuildCardTextures();
        CreateCreature();
        creatureCount = numCreatures;
        SpawnCreatures(numCreatures);
    }

    void Update()
    {
        if (numCreatures != creatureCount)
        {
            UpdateCreatures();
            creatureCount = numCreatures;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            Scatter();

        UpdateTrails();
        CalculateFlockingForces();

        float dt = Time.deltaTime;
        foreach (Creature c in creatures)
            c.UpdateCreatures(dt);
    }

    // ---------------------------------------------------------------
    // Build one texture per suit – drawn programmatically so no assets needed
    // ---------------------------------------------------------------
    void BuildCardTextures()
    {
        int W = 128, H = 192;
        cardTextures = new Texture2D[4];

        for (int s = 0; s < 4; s++)
        {
            Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            // White card background
            Color[] pixels = new Color[W * H];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            // Thin border
            Color border = new Color(0.7f, 0.7f, 0.7f);
            for (int x = 0; x < W; x++)
            {
                pixels[x]              = border; // bottom row
                pixels[(H-1)*W + x]    = border; // top row
            }
            for (int y = 0; y < H; y++)
            {
                pixels[y*W]        = border; // left col
                pixels[y*W + W-1]  = border; // right col
            }

            // Colour pip squares in corners and centre
            Color pip = suitColors[s];
            DrawFilledRect(pixels, W, H,  4,  4, 18, 18, pip);           // bottom-left
            DrawFilledRect(pixels, W, H, W-22, H-22, 18, 18, pip);       // top-right (mirrored)
            DrawFilledRect(pixels, W, H, W/2-12, H/2-12, 24, 24, pip);   // centre

            tex.SetPixels(pixels);
            tex.Apply();
            cardTextures[s] = tex;
        }
    }

    static void DrawFilledRect(Color[] pixels, int W, int H,
                               int x0, int y0, int w, int h, Color c)
    {
        for (int dy = 0; dy < h; dy++)
        for (int dx = 0; dx < w; dx++)
        {
            int px = x0 + dx, py = y0 + dy;
            if (px >= 0 && px < W && py >= 0 && py < H)
                pixels[py * W + px] = c;
        }
    }

    // ---------------------------------------------------------------
    // Build the shared card prefab (flat quad / thin box)
    // ---------------------------------------------------------------
    void CreateCreature()
    {
        creaturePrefab = new GameObject("CardPrefab");

        // Card body – standard playing-card aspect ratio ≈ 0.714
        GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
        card.transform.SetParent(creaturePrefab.transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale    = new Vector3(0.5f, 0.7f, 0.02f); // thin flat card

        // Give it a plain white material; texture applied per-instance at spawn
        Renderer rend = card.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        rend.material.color = Color.white;

        creaturePrefab.SetActive(false);
    }

    // ---------------------------------------------------------------
    void SpawnCreatures(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(creaturePrefab, transform);
            obj.SetActive(true);
            obj.name = "Card_" + i;

            obj.transform.position = new Vector3(
                Random.Range(-worldBounds.x / 2, worldBounds.x / 2),
                Random.Range(-worldBounds.y / 2, worldBounds.y / 2),
                Random.Range(-worldBounds.z / 2, worldBounds.z / 2)
            );

            // Pick a random suit and assign its texture
            int suit = Random.Range(0, 4);
            Renderer rend = obj.GetComponentInChildren<Renderer>();
            rend.material.SetTexture("_BaseMap", cardTextures[suit]);
            // Tint red suits slightly warmer, black suits slightly cool
            rend.material.SetColor("_BaseColor", (suit < 2)
                ? new Color(1f, 0.97f, 0.97f)
                : new Color(0.96f, 0.97f, 1f));

            Creature creature = obj.AddComponent<Creature>();
            creature.Initialize(this);

            // Random persistent spin axis & initial orientation
            creature.rotationOffset = new Vector3(
                Random.Range(-15f, 15f),   // slight tilt, not full tumble
                Random.Range(0f, 360f),    // random face direction
                Random.Range(-15f, 15f)
            );
            obj.transform.eulerAngles = creature.rotationOffset;

            creatures.Add(creature);
        }
    }

    // ---------------------------------------------------------------
    void UpdateCreatures()
    {
        int diff = numCreatures - creatures.Count;
        if (diff > 0)
        {
            SpawnCreatures(diff);
        }
        else if (diff < 0)
        {
            for (int i = 0; i < -diff; i++)
            {
                if (creatures.Count > 0)
                {
                    Creature toRemove = creatures[creatures.Count - 1];
                    creatures.RemoveAt(creatures.Count - 1);
                    Destroy(toRemove.gameObject);
                }
            }
        }
    }

    // ---------------------------------------------------------------
    void CalculateFlockingForces()
    {
        foreach (Creature creature in creatures)
        {
            Vector3 totalForce = Vector3.zero;
            List<Creature> neighbors  = new List<Creature>();
            List<Creature> collisions = new List<Creature>();

            foreach (Creature other in creatures)
            {
                if (other == creature) continue;
                float dist = Vector3.Distance(
                    creature.transform.position, other.transform.position);

                if (dist < avoidanceRadius)      collisions.Add(other);
                else if (dist < neighborRadius)  neighbors.Add(other);
            }

            if (flockCentering && neighbors.Count > 0)
            {
                Vector3 center = Vector3.zero;
                foreach (Creature n in neighbors) center += n.transform.position;
                center /= neighbors.Count;
                totalForce += (center - creature.transform.position).normalized
                              * centeringWeight;
            }

            if (velocityMatching && neighbors.Count > 0)
            {
                Vector3 avgVel = Vector3.zero;
                foreach (Creature n in neighbors) avgVel += n.velocity;
                avgVel /= neighbors.Count;
                totalForce += (avgVel - creature.velocity) * velocityWeight;
            }

            if (collisionAvoidance && collisions.Count > 0)
            {
                Vector3 avoidance = Vector3.zero;
                foreach (Creature other in collisions)
                {
                    Vector3 diff = creature.transform.position - other.transform.position;
                    float   dist = diff.magnitude;
                    if (dist > 0.01f) avoidance += diff.normalized / dist;
                }
                totalForce += avoidance * avoidanceWeight;
            }

            if (wandering)
            {
                Vector3 rnd = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)).normalized;
                totalForce += rnd * wanderingWeight;
            }

            creature.Force(totalForce);
        }
    }

    // ---------------------------------------------------------------
    void Scatter()
    {
        foreach (Creature c in creatures) c.Scatter();
    }

    void UpdateTrails()
    {
        foreach (Creature c in creatures) c.EnableTrail(trails);
    }

    // Exposed so Creature can read it for spin
    public float CardSpinSpeed => cardSpinSpeed;
}