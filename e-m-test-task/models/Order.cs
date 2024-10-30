using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

public class Order
{
    public int OrderId {get;set;} // идентификатор заказа
    public float Weight {get;set;} // вес
    public DateTime? OrderTime {get;set;} //время заказа
    public DateTime? ExpectedDeliveryTime {get;set;} //ожидаемое время доставки
    public DateTime? DeliveryTime {get;set;} //время доставки (решил добавить этот параметр. Если он будет null или совпадать с OrderTime - то заказ не будет считаться доставлен)
    [ForeignKey("District")]
    public int DistrictId {get; set;} // идентификатор района города
    public string? Ip {get; set;} // IP адрес. Для старого варианта ТЗ
}