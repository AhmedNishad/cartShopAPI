
var editableItems = document.getElementsByClassName("editable-item")
var editingFormField = document.getElementById("editing-form-field")
editingFormField.style.display = 'none'
var editButton = document.getElementById('edit-line-item')
var submitButton = document.getElementById('submit-button')
var productPickerNew = document.getElementById('product-picker-new')
var productPickerEdit = document.getElementById('product-picker-edit')

let showingEdit = false
var lineItemContainer = document.getElementById("line-item-container");
var addLineItem = document.getElementById("add-line-item");
var productInput = document.getElementById("product-input")
var productSelect = document.getElementById('product-select')

var productQuantity = document.getElementById("product-quantity") 
var idCounter = 0
var quantityInputNew = document.getElementById("quantity-input-new");
var quantityInputEdit = document.getElementById("quantity-input-edit");

var selectedProduct;
var totalOutput = document.getElementById("total-output")
var removeButtons = document.getElementsByClassName('remove-button')



function getSelectedOptionElement(selectedId) {
    for (var p = 0; p < productPickerNew.options.length; p++) {
        var productElem = productPickerNew.options[p]
        
            
        if (productElem.value == selectedId) {
            return productElem

        }
    }
    return null
    
}

for (var j = 0; j < removeButtons.length; j++) {
    removeButtons[j].addEventListener('click', removeLineItem);
}

submitButton.disabled = true;

addLineItem.disabled = true


productInput.addEventListener('input', (e) => {
   if (e.target.value.length > 1) {
        console.log(productPickerNew.value)
        selectedProduct = getSelectedOptionElement(productPickerNew.value)
    }
}
)

productSelect.addEventListener('change', (e) => {
    console.log(productSelect.value)
    selectedProduct = getSelectedOptionElement(productSelect.value)
    
}
)

quantityInputNew.addEventListener('input', (e) => {
    console.log(selectedProduct)
    quantityInputNew.max = selectedProduct.getAttribute('data-unit-quantity')
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInputNew.max) {
        addLineItem.disabled = true
    } else {
        addLineItem.disabled = false
    }
})

quantityInputEdit.addEventListener('input', (e) => {
    console.log(selectedProduct)
    
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInputEdit.max) {
        editButton.disabled = true
    } else {
        editButton.disabled = false
    }
})

for (var i = 0; i < editableItems.length; i++) {
    editableItems[i].addEventListener("click", editingItem)
    editableChildren = editableItems[i].children
}

function editingItem(e) {
    console.log(e.target.parentElement.getAttribute('data-product-name'))
    editingFormField.children[0].firstElementChild.innerText = e.target.parentElement.getAttribute('data-product-name')
    editingFormField.setAttribute('price', e.target.parentElement.getAttribute('data-product-price'))
    console.log(editingFormField.children[0].firstElementChild.children[2])
    console.log(editingFormField.children)
    editingFormField.setAttribute('data-product-id',e.target.parentElement.getAttribute('data-product-id'))
    //editingFormField.children[1].children[1].value = e.target.parentElement.getAttribute('data-line-item-id')
    e.target.parentElement.insertAdjacentElement('afterend', editingFormField)
    editingFormField.style.display = 'block'
    console.log(e.target.parentElement.getAttribute('data-product-id'))
    //editingFormField.children[0].firstElementChild.value = e.target.parentElement.getAttribute('data-product-id')
    editingFormField.children[1].firstElementChild.value = parseInt(e.target.parentElement.children[1].innerText)
    editingFormField.children[1].firstElementChild.max = e.target.parentElement.getAttribute('data-product-quantity')
    editButton.disabled = false
    //editingFormField.children[0].firstElementChild.children[1].value = e.target.parentElement.children[0].innerText
   // editingFormField.children[0].firstElementChild.children[0].value = e.target.parentElement.getAttribute('data-product-id')
}

editButton.addEventListener('click', editLineItem)

function editLineItem(e) {
    e.preventDefault()
    var newlyAddedQuantity = e.target.parentElement.parentElement.children[1].firstElementChild.value
    var modifyingRow = findModifyingRow(e.target.parentElement.parentElement.getAttribute('data-product-id'))
    var quantityDisplay = modifyingRow.children[1]
    quantityDisplay.innerText = newlyAddedQuantity

}

function findModifyingRow(productId) {
    for (var o = 0; o < lineItemContainer.children.length; o++) {
        var possibleRow = lineItemContainer.children[o]
        if (possibleRow.getAttribute('data-product-id') == productId)
            return possibleRow
    }
    return null
}

function addIdCounter() {
    var orderId = 0
    var lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        var child = lineItems[i]
        if (child.getAttribute('id') != 'editing-form-field') {
            grandChildren = child.children
            grandChildren[0].firstElementChild.setAttribute("name", `[${orderId}].product.id`)
            grandChildren[1].firstElementChild.setAttribute("name", `[${orderId}].quantity`)
            grandChildren[2].firstElementChild.setAttribute("name", `[${orderId}].total`)
            orderId++

        }
    }
}

addLineItem.addEventListener('click', e => {
    selectedProduct = getSelectedOptionElement(productPickerNew.parentElement.children[2].value)
    e.preventDefault();
    //if(e.target.)
    var lineItemElement = document.createElement("tr");
    var productIdElement = document.createElement("td");

    console.log(selectedProduct)
    console.log(productPickerNew.options[productPickerNew.parentElement.children[2].selectedIndex])
    
    productIdElement.innerHTML = `<input type='hidden' value='${selectedProduct.value}' name='[${idCounter}].product.id' /> 
                                 <h6>${selectedProduct.getAttribute('data-product-name')}</h6>`

    var quantityElement = document.createElement("td");
    quantityElement.innerHTML = `<input type="hidden" value="${parseInt(quantityInputNew.value)}" name="[${idCounter}].quantity" /> 
                                <h6>${quantityInputNew.value}</h6>`

    var totalElement = document.createElement("td");

    var lineTotal = (parseInt(selectedProduct.getAttribute('data-unit-price')) * parseInt(quantityInputNew.value));

    var itemAlreadyExists = checkIfPreviouslySubmitted(selectedProduct.value, quantityInputNew.value, lineTotal)

    if (!itemAlreadyExists) {
        totalElement.innerHTML = `<input type="hidden" value="${lineTotal}" name="[${idCounter}].total" /> <h6>${lineTotal}</h6>`
        var removeElement = document.createElement("td");
        var removeButton = document.createElement("button")
        removeButton.innerText = "Remove"
        console.log(removeButton)
        removeButton.classList.add("btn", "btn-danger")

        // Update Product quantity for element. Now obsolete because we're overriding updated elements
        //  productPicker.options[productPicker.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInput.max) - parseInt(quantityInput.value))

        removeButton.addEventListener('click', removeLineItem)
        removeElement.appendChild(removeButton)

        addLineItem.setAttribute('data-product-quantity', selectedProduct.getAttribute('data-product-quantity'))
        addLineItem.setAttribute('data-product-name', selectedProduct.getAttribute('data-product-name'))
        addLineItem.setAttribute('data-product-price', selectedProduct.getAttribute('data-product-price'))
        addLineItem.setAttribute('data-product-id', selectedProduct.getAttribute('data-product-id'))

        lineItemElement.classList.add('editable-item')
        lineItemElement.appendChild(productIdElement);
        lineItemElement.appendChild(quantityElement);
        lineItemElement.appendChild(totalElement);
        lineItemElement.appendChild(removeElement);
        lineItemContainer.appendChild(lineItemElement);
        idCounter++
        addIdCounter()
        updateTotal()
        checkSubmit()
        quantityInputNew.value = ""
        productInput.value = ""
        addLineItem.disabled = true
    }
})

function checkIfPreviouslySubmitted(productId, quantity, lineTotal) {
    for (var i = 0; i < lineItemContainer.children.length; i++) {
        var hiddenProductInput = lineItemContainer.children[i].firstElementChild.firstElementChild
        var productQuantityHiddenInput = lineItemContainer.children[i].children[1].firstElementChild
        var productQuantity = lineItemContainer.children[i].children[1].children[1]
        var lineTotalHiddenInput = lineItemContainer.children[i].children[2].firstElementChild
        var totalOfLine = lineItemContainer.children[i].children[2].children[1]
        if (hiddenProductInput.value == productId) {
            productQuantityHiddenInput.value = quantity
            productQuantity.innerText = quantity
            lineTotalHiddenInput.value = lineTotal
            totalOfLine.innerText = lineTotal;
            updateTotal()
            quantityInputNew.value = ""
            console.log("Exists")
            return true
        }
    }
    return false;
}

function updateTotal() {
    var total = 0;
    let lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        let child = lineItems[i]
        if (child.getAttribute('id') != 'editing-form-field')
        total += parseInt(child.children[2].firstElementChild.value)
    }
    totalOutput.innerText = total
}

function removeLineItem(e) {
    e.preventDefault()
    var toRemove = e.target.parentElement.parentElement;
    productPickerNew.value = toRemove.getAttribute('data-product-id')
    
    productPickerNew.options[productPickerNew.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInputNew.max) + parseInt(toRemove.children[1].firstElementChild.value))
    lineItemContainer.removeChild(toRemove);
    addIdCounter()
    updateTotal()
    checkSubmit()
}

function checkSubmit() {
    if (lineItemContainer.children.length > 0) {
        submitButton.disabled = false
    } else {
        submitButton.disabled = true
    }
}
