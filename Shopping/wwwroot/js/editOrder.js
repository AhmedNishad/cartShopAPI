
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
    editingFormField.children[1].firstElementChild.value = ""
    console.log(editingFormField.children[0].firstElementChild.children[2])
    if (!e.target.getAttribute("form-element", "true")) {
        console.log(editingFormField.children)
        editingFormField.children[1].children[1].value = e.target.parentElement.getAttribute('data-line-item-id')
        e.target.parentElement.insertAdjacentElement("afterend",editingFormField)
        editingFormField.style.display = 'block'
        console.log(e.target.parentElement.getAttribute('data-product-id'))
        editingFormField.children[0].firstElementChild.value = e.target.parentElement.getAttribute('data-product-id')
        editingFormField.children[1].firstElementChild.value = parseInt(e.target.parentElement.children[1].innerText)
        editingFormField.children[0].firstElementChild.children[1].value = e.target.parentElement.children[0].innerText
        editingFormField.children[0].firstElementChild.children[0].value = e.target.parentElement.getAttribute('data-product-id')
    }
}