<script>
    import Materialize from "materialize-css"
    
    export let placeholder;
    export let id;
    export let label_text;
    export let input_text;
    export let enabled = true;
</script>

<div class="input-field">
    <label class="unselectable" for={id}>{label_text}</label>
    <input type="text" id={id} bind:value={input_text} disabled={!enabled}>
</div>

<style>
    .unselectable {
        -webkit-touch-callout: none;
        -webkit-user-select: none;
        -khtml-user-select: none;
        -moz-user-select: none;
        -ms-user-select: none;
        -o-user-select: none;
        user-select: none;
    }
</style>
