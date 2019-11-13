
var editableItems = document.getElementsByClassName("editable-item")
var editingFormField = document.getElementById("editing-form-field")
editingFormField.hidden = true
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
submitButton.disabled = true;

addLineItem.disabled = true


productInput.addEventListener('input', (e) => {
    editingFormField.hidden = true
   if (e.target.value.length > 1) {
        console.log(productPickerNew.value)
       selectedProduct = getSelectedOptionElementByInput(productPickerNew.value) //getSelectedOptionElement(productPickerNew.value)
    }
}
)

productSelect.addEventListener('change', (e) => {
    console.log(productSelect.value)
    selectedProduct = getSelectedOptionElement(productSelect.value)
    
}
)

quantityInputNew.addEventListener('input', (e) => {
    editingFormField.hidden = true

    console.log(selectedProduct)
    quantityInputNew.max = selectedProduct.getAttribute('data-unit-quantity')
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInputNew.max) {
        addLineItem.disabled = true
    } else {
        addLineItem.disabled = false
    }
})

quantityInputEdit.addEventListener('input', (e) => {
    
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
    var tableRow = e.target.parentElement.parentElement
    console.log(tableRow)
    if (tableRow.classList.contains('editable-item') && !e.target.classList.contains('remove-button')) {
        editingFormField.hidden = false

        editingFormField.children[0].firstElementChild.innerText = tableRow.getAttribute('data-product-name')
        editingFormField.setAttribute('price', tableRow.getAttribute('data-product-price'))
        
       
        editingFormField.setAttribute('data-product-id', tableRow.getAttribute('data-product-id'))
        tableRow.insertAdjacentElement('afterend', editingFormField)
       // editingFormField.style.display = 'block'
       
        editingFormField.children[1].firstElementChild.value = parseInt(tableRow.children[1].innerText)
        editingFormField.children[1].firstElementChild.max = tableRow.getAttribute('data-product-quantity')
        editButton.disabled = false
        console.log(editingFormField)

    }
}

editButton.addEventListener('click', editLineItem)

function editLineItem(e) {
    e.preventDefault()
    
    var newlyAddedQuantity = e.target.parentElement.parentElement.children[1].firstElementChild.value
    console.log(editingFormField.getAttribute('data-product-id'))
    var modifyingRow = findModifyingRow(editingFormField.getAttribute('data-product-id'))
    var newPrice = parseInt(newlyAddedQuantity) * parseInt(modifyingRow.getAttribute('data-product-price'))
    var quantityDisplay = modifyingRow.children[1].children[1]
    var priceDisplay = modifyingRow.children[2].children[1]
    var quantityInput = modifyingRow.children[1].firstElementChild
    var priceInput = modifyingRow.children[2].firstElementChild
    quantityDisplay.innerText = newlyAddedQuantity
    priceDisplay.innerText = newPrice
    priceInput.value = parseInt(newPrice)
    quantityInput.value = parseInt(newlyAddedQuantity)
    checkSubmit()
}

function findModifyingRow(productId) {
    for (var o = 0; o < lineItemContainer.children.length; o++) {
        var possibleRow = lineItemContainer.children[o]
        if (possibleRow.getAttribute('data-product-id') == productId) {

            console.log(possibleRow)
            return possibleRow
        }
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
            if (grandChildren[0].children[2] != null) {
                grandChildren[0].children[2].setAttribute("name", `[${orderId}].id`)

            }
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
        lineItemElement.addEventListener("click", editingItem)
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
    debugger
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

for (var j = 0; j < removeButtons.length; j++) {
    removeButtons[j].addEventListener('click', removeLineItem);
}

function removeLineItem(e) {
    e.preventDefault()
    var toRemove = e.target.parentElement.parentElement;
    productPickerNew.value = toRemove.getAttribute('data-product-id')
    editingFormField.hidden = true
    
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
