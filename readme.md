![Alt text](PackagePartitioner.png?raw=true "Title")

1.	PackagePartitioner масштабируется горизонтально. Запускает продьюсеров по числу топиков.
	a.	Sharding по хешу коробки определяет топик и соответственно ноду БД
2.	Кафка содержит 1 топик на 1 физический узел БД
	a.	пишет идепотентно исключая дубликаты и ожидает подтверждения персистентности
3.	PackageConsumer по одному на топик и соотвественно на узел БД
	a.	Хранит свой offset в БД
	b.	Так как является единственным писателем в БД избавляет от гонки 
4.	Запись в БД происходит транзакционно
	a.	Получаем несколько записей из очереди
	b.	В одной транзакции пишем в таблицу и оффсет в БД
	c.	После комита транзакции комитим оффсет в очередь
5.	Коробку находи одним запросом в БД
	a.	Sharding по хешу коробки определяет ноду БД
6.	Выборку по вторичному индексу (containerId) в данной реализации необходимо производить на всех узлах
	a.	 можно реализовать поддержку 2х primary индексов, две таблицы: boxId/containerId и containerId/boxId но это сильно усложнит путь записи и поддержку консистентности

Перебалансировка:
1.	Описана в классе Sharding.Map ()
	a.	Создаем секций больше чем физических узлов (ScaleFactor = 10)
	b.	Поддерживаем маппинг секций на узлы
	c.	При добавлении физических узлов – обновляем маппинг
	d.	Поддерживаем две версии маппинга пока идет перемещение данных
2.	При добавлении физических узлов нужно отскелить соотвественно:
	a.	Продьюсеров в PackagePartitioner
	b.	Топики в кафка
	c.	Инстансы PackageConsumer
3.	Хранить маппинг предпочтитетльно в централизованном хранилище (zookeeper, consul и тд)
