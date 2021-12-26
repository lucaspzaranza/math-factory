using UnityEngine;
using UnityEngine.UI;

public class SelectionRect : MonoBehaviour
{
    public static SelectionRect instance;
    [SerializeField] public Sprite[] rectSprites;
    private Vector3 initRectPos;
    private GameControl GCtrl { get { return GameControl.instance; }}
    private GameUI UICtrl { get { return GameUI.instance; }}
    [SerializeField] private RectTransform rectTransform;

    void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    void Start()
    {
        initRectPos = transform.position;
        rectTransform = GetComponent<RectTransform>();
    }

    public void ChangeRectPosition(int pos)
    {
        float offset = 1.59f;
        float newX = initRectPos.x - (pos * offset);
        transform.position = new Vector3(newX, transform.position.y, 0f);
    }

    public void ChangeRectRow(int row)
    {        
        rectTransform.SetParent(UICtrl.slotsParents[row], false);
        transform.SetSiblingIndex(0);
    }

    public void ChangeRectSize(int slotCount)
    {
        // offset = 115 | border = 5f
        float newWidth = (slotCount * 115f) + 5f;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }
}