using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShelfPanel : MonoBehaviour
{
    [SerializeField] private List<ChapterBookPanel> bookList;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public List<ChapterBookPanel> GetBookList()
    {
        return bookList;
    }
}