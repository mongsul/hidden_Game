using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public void IPointerEnterHandler()
    {
        //OnPointerEnter �����Ͱ� ������Ʈ �ȿ� ���� ����
    }
    public void IPointerExitHandler()
    {
        //OnPointerExit �����Ͱ� ������Ʈ ���� ���ö� ȣ��
    }
    public void IPointerDownHandler()
    {
        //OnPointerDown �����Ͱ� ������Ʈ ������ ������ �� ȣ��
    }
    public void IPointerUpHandler()
    {
        //OnPointerUp �����͸� ������Ʈ ���� �� �� ȣ��
    }
    public void IPointerClickHandler()
    {
        //OnPointerClick ���� ������Ʈ���� �����͸� ������ �� �� ȣ��
    }
    public void IInitializePotentialDragHandler()
    {
        //OnInitializePotentialDrag �巡�� Ÿ���� �߰ߵǾ����� ȣ��, ���� �ʱ�ȭ �� �� ����� �� ����.
    }
    public void IBeginDragHandler()
    {
        //OnBeginDrag �巡�װ� ���۵Ǵ� ������ �巡�� ��� ������Ʈ���� ȣ��.
    }
    public void IDragHandler()
    {
        //OnDrag   �巡�� ������Ʈ�� �巡�׵Ǵ� ���� ȣ��
    }
    public void IEndDragHandler()
    {
        //OnEndDrag   �巡�װ� ������� �� �巡�� ������Ʈ���� ȣ��
    }
    public void IDropHandler()
    {
        //OnDrop  �巡�׸� ������ �� �ش� ������Ʈ���� ȣ��
    }
    public void IScrollHandler()
    {
        //OnScroll  ���콺 ���� ��ũ������ �� ȣ��
    }
    public void IUpdateSelectedHandler()
    {
        //OnUpdateSelected ������ ������Ʈ���� �� ƽ���� ȣ��
    }
    public void ISelectHandler()
    {
        //OnSelect  ������Ʈ�� �����ϴ� ���� ȣ��
    }
    public void IDeselectHandler()
    {
        //OnDeselect   ������ ������Ʈ�� ���� ������ �� ȣ��
    }
    public void IMoveHandler()
    {
        //OnMove   �̵� �̺�Ʈ(����, ������, ����, �Ʒ��� ��)�� �߻����� �� ȣ��
    }
    public void ISubmitHandler()
    {
        //OnSubmit    ���� ��ư�� ������ �� ȣ��
    }
    public void ICancelHandler()
    {
        //OnCancel    ��� ��ư�� ������ �� ȣ��
    }
}
