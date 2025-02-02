export function GetSvgClickCoordinates(refElementSvg, clientX, clientY) {
    let pt = DOMPoint.fromPoint(refElementSvg);
    pt.x = clientX;
    pt.y = clientY;
    let svgPointCoordinates = pt.matrixTransform(refElementSvg.getScreenCTM().inverse());
    return [svgPointCoordinates.x, svgPointCoordinates.y];
}