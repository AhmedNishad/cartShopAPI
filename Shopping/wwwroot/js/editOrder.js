
var editableItems = document.getElementsByClassName("editable-item")
var editingFormField = document.getElementById("editing-form-field")
editingFormField.style.display = 'none'
var editButton = document.getElementById('edit-button')
var productPicker = document.getElementById('product-picker')
let showingEdit = false


for (let i = 0; i < editableItems.length; i++) {
    editableItems[i].addEventListener("click", editingItem)
    editableChildren = editableItems[i].children
    for (let i = 0; i < editableChildren.length; i++) {

    }
}



function addEditedElement(e) {
   // e.preventDefault()
}

function editingItem(e) {

    console.log(e.target)
   
    if (!e.target.getAttribute("form-element", "true")) {
        editingFormField.children[1].children[1].value = e.target.parentElement.getAttribute('data-line-item-id')
        e.target.parentElement.insertAdjacentElement("afterend",editingFormField)
        editingFormField.style.display = 'block'
        console.log(e.target.parentElement.getAttribute('data-product-id'))
        editingFormField.children[0].firstElementChild.value = e.target.parentElement.getAttribute('data-product-id')
        editingFormField.children[1].firstElementChild.value = parseInt(e.target.parentElement.children[1].innerText)
    }
}