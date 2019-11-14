var datePicker = document.getElementById("date-picker");
var date = new Date();
var day = date.getDate().toString().length > 1 ? date.getDate() : "0" + date.getDate().toString()
var month = date.getMonth().toString().length > 1 ? date.getMonth() : "0" + date.getMonth().toString()
datePicker.value = `${date.getFullYear()}-${date.getMonth() + 1}-${day}`;
var productPicker = document.getElementById("product-picker");
var productInput = document.getElementById("product-input")
var quantityInput = document.getElementById("quantity-input");
var quantityOutput = document.getElementById("quantity-output");
var addLineItem = document.getElementById("add-line-item");
var lineItemContainer = document.getElementById("line-item-container");
var totalOutput = document.getElementById("total-output");
var idCounter = 0;
var submitButton = document.getElementById("submit-button");
var customerInput = document.getElementById("customer-input");

checkSubmit()
addLineItem.disabled = true

// Add button functionality checks
quantityInput.addEventListener('input', (e) => {
    quantityInput.max = productPicker.options[productPicker.selectedIndex].getAttribute('data-unit-quantity')
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInput.max) {
        addLineItem.disabled = true
    } else {
        addLineItem.disabled = false
    }
})

// Submit button checks
customerInput.addEventListener('input', (e) => {
    if (customerInput.value == "" && customerInput.placeholder == "") {
        submitButton.disabled = true;
    }
})

// Remove all hidden inputs
function addFunctionality() {
    if (lineItemContainer.children.length ) {
        let tableRows = lineItemContainer.children
        for (let i = 0; i < tableRows.length; i++) {
            tableRows[i].children[3].firstElementChild.addEventListener('click', removeLineItem);
        }
    }
}
addFunctionality()

addLineItem.addEventListener('click', e => {
    e.preventDefault();
    var lineItemElement = document.createElement("tr");
    var productIdElement = document.createElement("td");
    productIdElement.innerHTML = `<input type='hidden' value='${productPicker.value}' name='[${idCounter}].product.id' /> 
                                 <h6>${productPicker.options[productPicker.selectedIndex].getAttribute('data-product-name')}</h6>`

    var quantityElement = document.createElement("td");
    quantityElement.innerHTML = `<input type="hidden" value="${parseInt(quantityInput.value)}" name="[${idCounter}].quantity" /> 
                                <h6>${quantityInput.value}</h6>`

    var totalElement = document.createElement("td");

    var lineTotal = (parseInt(productPicker.options[productPicker.selectedIndex].getAttribute('data-unit-price')) * parseInt(quantityInput.value));

    let itemAlreadyExists = checkIfPreviouslySubmitted(productPicker.value, quantityInput.value, lineTotal)

    if (!itemAlreadyExists) {
        totalElement.innerHTML = `<input type="hidden" value="${lineTotal}" name="[${idCounter}].total" /> <h6>${lineTotal}</h6>`
        var removeElement = document.createElement("td");
        var removeButton = document.createElement("button")
        removeButton.innerText = "Remove"
        removeButton.classList.add("btn", "btn-danger")

        // Update Product quantity for element. Now obsolete because we're overriding updated elements
      //  productPicker.options[productPicker.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInput.max) - parseInt(quantityInput.value))

        removeButton.addEventListener('click', removeLineItem)
        removeElement.appendChild(removeButton)

        lineItemElement.appendChild(productIdElement);
        lineItemElement.appendChild(quantityElement);
        lineItemElement.appendChild(totalElement);
        lineItemElement.appendChild(removeElement);
        lineItemContainer.appendChild(lineItemElement);
        idCounter++
        addIdCounter()
        updateTotal()
        checkSubmit()
        quantityInput.value = ""
        productInput.value = ""
        addLineItem.disabled = true
        productInput.focus()
    }
})

// If item of same product previously added. Replace new quantity and total
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
            quantityInput.value = ""
            return true
        }
    }
    return false;
}

// Update total display at bottom of page
function updateTotal() {
    var total = 0;
    let lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        let child = lineItems[i]
        total += parseInt(child.children[2].firstElementChild.value)
    }
    totalOutput.innerText = total
}

// Update hidden input names to accomodate model binding
function addIdCounter() {
    var orderId = 0
    var lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        var child = lineItems[i]
        grandChildren = child.children
        grandChildren[0].firstElementChild.setAttribute("name", `[${orderId}].product.id`)
        grandChildren[1].firstElementChild.setAttribute("name", `[${orderId}].quantity`)
        grandChildren[2].firstElementChild.setAttribute("name", `[${orderId}].total`)
        orderId++
    }
}

// Remove the table row including hidden inputs
function removeLineItem(e) {
    e.preventDefault()
    var toRemove = e.target.parentElement.parentElement;
    productPicker.options[productPicker.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInput.max) + parseInt(toRemove.children[1].firstElementChild.value))
    lineItemContainer.removeChild(toRemove);
    addIdCounter()
    updateTotal()
    checkSubmit()
    productInput.focus()
}

// Check if the submit should be disabled. (If line items list is empty)
function checkSubmit() {
    if (lineItemContainer.children.length > 0 && (customerInput.value != "" && customerInput.placeHolder != "")) {
        submitButton.disabled = false
    } else {
        submitButton.disabled = true
    }
}





