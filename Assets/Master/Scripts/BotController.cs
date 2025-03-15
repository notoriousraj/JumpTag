using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    public Transform player;
    public float safeDistance = 10f;
    public float moveSpeed = 5f;
    public float panicSpeedMultiplier = 2f;
    public float panicDistance = 3f;
    public float updateInterval = 0.2f;
    public float stuckCheckDistance = 1f;
    public float randomMoveRadius = 5f;
    public float jumpSpeed = 8f; // Speed while jumping between platforms

    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private float nextUpdateTime = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent is missing on " + gameObject.name);
            return;
        }

        agent.speed = moveSpeed;
        EnsureBotOnNavMesh();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (player == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        AdjustSpeedBasedOnDistance();

        if (Time.time >= nextUpdateTime)
        {
            if (IsStuck())
            {
                JumpToNewPlatform();
            }
            else
            {
                RunAwayFromPlayer();
            }
            nextUpdateTime = Time.time + updateInterval;
        }

        HandleJumping();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the "Player" tag
        {
            GameManager.Instance.BotCaught();
            Destroy(gameObject); // Destroy the bot on collision
        }
    }

    void RunAwayFromPlayer()
    {
        Vector3 direction = transform.position - player.position;
        direction.y = 0;

        Vector3 newTarget = transform.position + direction.normalized * safeDistance;
        Debug.Log("New Target Position: " + newTarget);

        if (NavMesh.SamplePosition(newTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("Bot couldn't find a valid path, attempting to jump.");
            JumpToNewPlatform();
        }
    }

    void AdjustSpeedBasedOnDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < panicDistance)
        {
            agent.speed = moveSpeed * panicSpeedMultiplier;
            updateInterval = 0.1f; // React faster when in danger
        }
        else
        {
            agent.speed = moveSpeed;
            updateInterval = 0.3f; // Slower updates when safe
        }
    }

    bool IsStuck()
    {
        return agent.velocity.magnitude < 0.1f; // Check if the bot is actually moving
    }

    void JumpToNewPlatform()
    {
        if (agent.isOnOffMeshLink) // If bot is on a NavMesh link, make it jump
        {
            StartCoroutine(JumpAcross());
        }
        else
        {
            Vector3 randomDirection = Random.insideUnitSphere * randomMoveRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    IEnumerator JumpAcross()
    {
        if (!agent.isOnOffMeshLink) yield break; // Prevent unnecessary jumps

        OffMeshLinkData link = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = link.endPos;
        float elapsedTime = 0f;
        float jumpDuration = 0.5f;

        agent.enabled = false; // Temporarily disable movement

        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * Mathf.Sin(t * Mathf.PI); // Arc-like jump
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        agent.enabled = true;
        agent.CompleteOffMeshLink(); // Mark the link as completed
    }

    void HandleJumping()
    {
        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcross());
        }
    }

    void EnsureBotOnNavMesh()
    {
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.enabled = true;
            }
            else
            {
                Debug.LogError("Bot is outside NavMesh! Adjust spawn position.");
            }
        }
    }
}
