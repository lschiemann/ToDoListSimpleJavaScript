const listsUri = 'api/lists';
const itemsUri = '/items';
let todos = [];
let lists = [];

function getLists() {
  fetch(listsUri)
    .then(response => response.json())
    .then(data => _displayLists(data))
    .catch(error => console.error('Unable to get lists.', error));
}

function getItems(listId) {
  fetch(_getItemsUri(listId))
    .then(response => response.json())
    .then(data => _displayItems(data, listId))
    .catch(error => console.error('Unable to get items.', error));
}

function addItem(listId) {
  const addTitleTextbox = document.getElementById('add-title');
  let text = JSON.stringify(addTitleTextbox.value.trim());
  fetch(_getItemsUri(listId), {
    method: 'POST',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    },
    body: text
  })
    .then(response => response.json())
    .then(() => {
      getItems(listId);
      addTitleTextbox.value = '';
    })
    .catch(error => console.error('Unable to add item.', error));
}

function addList() {
  const addTitleTextbox = document.getElementById('add-list-title');
  let text = JSON.stringify(addTitleTextbox.value.trim());
  fetch(listsUri, {
    method: 'POST',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    },
    body: text
  })
    .then(response => response.json())
    .then(() => {
      getLists();
      addTitleTextbox.value = '';
    })
    .catch(error => console.error('Unable to add list.', error));
}

function deleteItem(itemId, listId) {
  fetch(`${_getItemsUri(listId)}/${itemId}`, {
    method: 'DELETE'
  })
    .then(() => getItems(listId))
    .catch(error => console.error('Unable to delete item.', error));
}

function deleteList(id) {
  fetch(`${listsUri}/${id}`, {
    method: 'DELETE'
  })
    .then(() => getLists())
    .catch(error => console.error('Unable to delete list.', error));
}

function displayEditForm(itemId, listId) {
  const item = todos.find(i => i.id === itemId);

  document.getElementById('edit-title').value = item.title;
  document.getElementById('edit-description').value = item.description;
  document.getElementById('edit-id').value = item.id;
  document.getElementById('edit-listId').value = listId;
  document.getElementById('edit-isCompleted').checked = item.isCompleted;
  document.getElementById('editForm').style.display = 'block';
}

function displayListEditForm(id) {
  const list = lists.find(l => l.id === id);

  document.getElementById('edit-list-title').value = list.title;
  document.getElementById('edit-list-id').value = list.id;
  document.getElementById('editListForm').style.display = 'block';
}

function updateItem() {
  const itemId = document.getElementById('edit-id').value;
  const listId = document.getElementById('edit-listId').value;
  const item = {
    id: parseInt(itemId, 10),
    isCompleted: document.getElementById('edit-isCompleted').checked,
    title: document.getElementById('edit-title').value.trim(),
    description: document.getElementById('edit-description').value.trim()
  };

  fetch(`${_getItemsUri(listId)}/${itemId}`, {
    method: 'PUT',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(item)
  })
    .then(() => getItems(listId))
    .catch(error => console.error('Unable to update item.', error));

  closeInput();

  return false;
}

function updateList() {
  const listId = document.getElementById('edit-list-id').value;
  const list = {
    id: parseInt(listId, 10),
    title: document.getElementById('edit-list-title').value.trim()
  };

  fetch(`${listsUri}/${listId}`, {
    method: 'PUT',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(list)
  })
    .then(() => getLists())
    .catch(error => console.error('Unable to update list.', error));

  closeListInput();

  return false;
}

function closeInput() {
  document.getElementById('editForm').style.display = 'none';
}

function closeListInput() {
  document.getElementById('editListForm').style.display = 'none';
}

function _getItemsUri(listId) {
  return `${listsUri}/${listId}${itemsUri}`;
}

function _displayCount(itemCount) {
  const name = (itemCount === 1) ? 'to-do' : 'to-dos';

  document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayListCount(listCount) {
  const name = (listCount === 1) ? 'to-do list' : 'to-do lists';

  document.getElementById('listCounter').innerText = `${listCount} ${name}`;
}

function _displayLists(data) {
  const tBody = document.getElementById('lists');
  tBody.innerHTML = '';

  var dataCount = data.length;
  _displayListCount(dataCount);

  var listsTable = document.getElementById('listsTable');
  dataCount === 0
    ? listsTable.style.display = 'none'
    : listsTable.style.display = 'table';

  const button = document.createElement('button');

  data.forEach(list => {
    let showButton = button.cloneNode(false);
    showButton.innerText = 'Show items';
    showButton.setAttribute('onclick', `getItems(${list.id})`);

    let editButton = button.cloneNode(false);
    editButton.innerText = 'Edit';
    editButton.setAttribute('onclick', `displayListEditForm(${list.id})`);

    let deleteButton = button.cloneNode(false);
    deleteButton.innerText = 'Delete';
    deleteButton.setAttribute('onclick', `deleteList(${list.id})`);

    let tr = tBody.insertRow();

    let td1 = tr.insertCell(0);
    let textNode = document.createTextNode(list.title);
    td1.appendChild(textNode);

    let td2 = tr.insertCell(1);
    td2.appendChild(showButton);

    let td3 = tr.insertCell(2);
    td3.appendChild(editButton);

    let td4 = tr.insertCell(3);
    td4.appendChild(deleteButton);
  });

  lists = data;
}

function _displayItems(data, listId) {
  document.getElementById('items').style.display = 'block';
  const tBody = document.getElementById('todos');
  tBody.innerHTML = '';

  var dataCount = data.length;
  _displayCount(dataCount);

  var itemsTable = document.getElementById('itemsTable');
  dataCount === 0
    ? itemsTable.style.display = 'none'
    : itemsTable.style.display = 'table';

  const button = document.createElement('button');

  data.forEach(item => {
    let isCompleteCheckbox = document.createElement('input');
    isCompleteCheckbox.type = 'checkbox';
    isCompleteCheckbox.disabled = true;
    isCompleteCheckbox.checked = item.isCompleted;

    let editButton = button.cloneNode(false);
    editButton.innerText = 'Edit';
    editButton.setAttribute('onclick', `displayEditForm(${item.id}, ${listId})`);

    let deleteButton = button.cloneNode(false);
    deleteButton.innerText = 'Delete';
    deleteButton.setAttribute('onclick', `deleteItem(${item.id}, ${listId})`);

    let tr = tBody.insertRow();

    let td1 = tr.insertCell(0);
    td1.appendChild(isCompleteCheckbox);

    let td2 = tr.insertCell(1);
    let textNode = document.createTextNode(item.title);
    td2.appendChild(textNode);

    let td3 = tr.insertCell(2);
    td3.appendChild(editButton);

    let td4 = tr.insertCell(3);
    td4.appendChild(deleteButton);
  });

  const form = document.getElementById('addItemForm');
  form.onsubmit = () => addItem(listId);

  //let addButton = button.cloneNode(false);
  //addButton.innerText = 'Add item';
  //addButton.setAttribute('onclick', `addItem(${listId})`);

  //let addText = document.createElement('input');
  //addText.id = 'add-title';
  //addText.type = 'text';
  //let tr = tBody.insertRow();
  //let td1 = tr.insertCell(0);
  //td1.colSpan = 3;
  //td1.appendChild(addText);
  //let td2 = tr.insertCell(1);
  //td2.appendChild(addButton);

  todos = data;
}