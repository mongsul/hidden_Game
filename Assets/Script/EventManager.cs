using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public void IPointerEnterHandler()
    {
        //OnPointerEnter 포인터가 오브젝트 안에 들어갈때 오출
    }
    public void IPointerExitHandler()
    {
        //OnPointerExit 포인터가 오브젝트 에서 나올때 호출
    }
    public void IPointerDownHandler()
    {
        //OnPointerDown 포인터가 오브젝트 위에서 눌렸을 때 호출
    }
    public void IPointerUpHandler()
    {
        //OnPointerUp 포인터를 오브젝트 에서 뗄 때 호출
    }
    public void IPointerClickHandler()
    {
        //OnPointerClick 동일 오브젝트에서 포인터를 누르고 뗄 때 호출
    }
    public void IInitializePotentialDragHandler()
    {
        //OnInitializePotentialDrag 드래그 타겟이 발견되었을대 호출, 값을 초기화 할 때 사용할 수 있음.
    }
    public void IBeginDragHandler()
    {
        //OnBeginDrag 드래그가 시작되는 시점에 드래그 대상 오브젝트에서 호출.
    }
    public void IDragHandler()
    {
        //OnDrag   드래그 오브젝트가 드래그되는 동안 호출
    }
    public void IEndDragHandler()
    {
        //OnEndDrag   드래그가 종료됐을 때 드래그 오브젝트에서 호출
    }
    public void IDropHandler()
    {
        //OnDrop  드래그를 멈췄을 때 해당 오브젝트에서 호출
    }
    public void IScrollHandler()
    {
        //OnScroll  마우스 휠을 스크롤했을 때 호출
    }
    public void IUpdateSelectedHandler()
    {
        //OnUpdateSelected 선택한 오브젝트에서 매 틱마다 호출
    }
    public void ISelectHandler()
    {
        //OnSelect  오브젝트를 선택하는 순간 호출
    }
    public void IDeselectHandler()
    {
        //OnDeselect   선택한 오브젝트를 선택 해제할 때 호출
    }
    public void IMoveHandler()
    {
        //OnMove   이동 이벤트(왼쪽, 오른쪽, 위쪽, 아래쪽 등)가 발생했을 때 호출
    }
    public void ISubmitHandler()
    {
        //OnSubmit    전송 버튼이 눌렸을 때 호출
    }
    public void ICancelHandler()
    {
        //OnCancel    취소 버튼이 눌렸을 때 호출
    }
}
