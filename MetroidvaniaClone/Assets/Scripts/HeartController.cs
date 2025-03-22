using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    PlayerController player;

    public Transform heartsParent;
    public GameObject heartCointainerPrefab;

    private GameObject[] heartContainers;
    private Image[] heartFills;

    // Start is called before the first frame update
    private void Start()
    {
        player = PlayerController.Instance;
        heartContainers = new GameObject[PlayerController.Instance.maxHealth];
        heartFills = new Image[player.maxHealth];

        player.onHealthChangedCallback += UpdateHeartsHUD;
        InstantiateHeartContainer();
        UpdateHeartsHUD();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if(i < player.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    private void SetFilledHearts()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < player.Health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    private void InstantiateHeartContainer()
    {
        for (int i = 0; i < player.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartCointainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }

    private void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }
}
